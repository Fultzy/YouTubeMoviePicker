using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Models.Extensions;
using YouTubeMoviePicker.Properties;

namespace YouTubeMoviePicker.Services;
internal class Scheduler
{
    private static Scheduler _instance;
    public static Scheduler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Scheduler();
            }
            return _instance;
        }
    }

    private Timer postingTimer;
    private Timer fetchingTimer;
    private Timer clearUnpickedTimer;
    private Timer clearPickedTimer;

    public void Start()
    {
        // Ensure we stop any active timers before starting new ones
        StopAllTimers();

        // Schedule each task with the current settings
        Logger.MainLog(" == Starting Scheduler ==", false);

        SchedulePostingTask();
        ScheduleFetchingTask(); // Data Operations have been moved here
        
        var test = ReadyToClearUnpickedMovies();
        var test2 = ReadyToClearPickedMovies();
    }

    private void StopAllTimers()
    {
        postingTimer?.Dispose();
        fetchingTimer?.Dispose();
        clearUnpickedTimer?.Dispose();
        clearPickedTimer?.Dispose();
    }

    ///////////// Posting /////////////
    private void SchedulePostingTask()
    {
        DateTime nextPostTime = GetNextPostDateTime();
        TimeSpan initialDelay = nextPostTime - DateTime.Now;

        Logger.MainLog($"Next movie  post in: {GetTimeToNextPostString()} => Targeting {nextPostTime.ToString("F")}");
        postingTimer = new Timer(ExecutePostingTask, null, initialDelay, TimeSpan.FromDays(1));  // Post daily
    }

    public string GetTimeToNextPostString()
    {
        DateTime nextPostTime = GetNextPostDateTime();
        var target = nextPostTime - DateTime.Now;
        
        var days = target.Days > 0 ? target.Days + "d " : "";
        var hours = target.Hours > 0 ? target.Hours + "h " : "";
        var minutes = target.Minutes > 0 ? target.Minutes + "m " : "";
        var seconds = target.Seconds > 0 ? target.Seconds + "s" : ""; 

        return $"{days}{hours}{minutes}{seconds}";
    }

    private async void ExecutePostingTask(object state)
    {
        Logger.MainLog("== Posting Movie ==", false);

        // Stop if no movies to pick from
        if (DataService.Instance.UnpickedMoviesList.Count == 0)
        {
            Logger.MainLog("No Movies to post!!");
            return;
        }

        // Stop if both WebHooks are disabled
        if (!Settings.Default.SendToDiscord && !Settings.Default.SendToSlack)
        {
            Logger.MainLog("No WebHooks enabled!!");
            return;
        }

        // select next movie in list
        var nextMovie = DataService.Instance.UnpickedMoviesList[0];

        var discordResult = "Failure"; // Default to failure
        var slackResult = "Failure";   // Default to failure

        // Try to send the movie to Discord
        if (Settings.Default.SendToDiscord && !string.IsNullOrEmpty(Settings.Default.DiscordWebHook))
        {
            discordResult = await DiscordService.SendPayloadAsync(nextMovie.ToDiscordPayload());
        }

        // Try to send the movie to Slack
        if (Settings.Default.SendToSlack && !string.IsNullOrEmpty(Settings.Default.SlackWebHook))
        {
            slackResult = await SlackService.SendPayloadAsync(nextMovie.ToSlackPayload(), nextMovie.Title, nextMovie.YTVideoURL);
        }

        // Check if any results are successful
        if (slackResult == "Successful" || discordResult == "Successful")
        {
            var msg1 = slackResult == "Successful" ? " Slack" : "";
            var msg2 = discordResult == "Successful" ? " Discord" : "";
            Logger.MainLog($"Posted {nextMovie.Title} to{msg1}{msg2} Successfully!");
            
            // Move the movie to the Picked Movies list
            DataService.Instance.PickNextMovie();
        }
    }

    private DateTime GetNextPostDateTime()
    {
        // Find the next valid day and time for posting
        var hour = Settings.Default.HourToPostIndex + 1;
        var hourAdj = Settings.Default.AmPmToPostIndex == 1 ? 12 : 0;
        var minute = Settings.Default.MinuteToPostIndex * 30;
        DateTime targetTime = DateTime.Today.AddHours(hour + hourAdj).AddMinutes(minute);

        // Check if the post time has already passed
        if (DateTime.Now > targetTime)
        {
            // Set target for the next day
            targetTime = targetTime.AddHours(24);

            // Remove any seconds and milliseconds to avoid stacking delays
            targetTime = targetTime.AddSeconds(-targetTime.Second).AddMilliseconds(-targetTime.Millisecond);
        }

        // Get the current day of the week Index
        int currentDayIndex = (int)DateTime.Now.DayOfWeek; // Sunday = 0, Saturday = 6
        
        // Check if the current day matches the allowed posting days
        // (SMTWHFA as XAAAAAX) 'A' represents available day
        while (Settings.Default.DaysToRun[currentDayIndex] != 'A')  
        {
            // Adjust target day (increment until matching the next available day)
            targetTime = targetTime.AddDays(1);
            currentDayIndex = (currentDayIndex + 1) % 7;  // Loop through the days
        }

        return targetTime;
    }

    ///////////// Fetching /////////////
    private void ScheduleFetchingTask()
    {
        // get the time to the next fetch
        DateTime nextFetchTime = GetNextFetchDateTime();
        TimeSpan initialDelay = nextFetchTime - DateTime.Now;

        Logger.MainLog($"Next movie fetch in: {GetTimeToNextFetchString()} => Targeting {nextFetchTime.ToString("F")}");

        fetchingTimer = new Timer(ExecuteFetchingTask, null, initialDelay, TimeSpan.FromDays(1));
    }
   
    private DateTime GetNextFetchDateTime()
    {
        var hour = Settings.Default.HourToFetchIndex + 1;
        var hourAdj = Settings.Default.AmPmToFetchIndex == 1 ? 12 : 0;
        var minute = Settings.Default.MinuteToFetchIndex * 30;
        DateTime targetTime = DateTime.Today.AddHours(hour + hourAdj).AddMinutes(minute);

        // Check if the fetch time has already passed
        if (DateTime.Now > targetTime)
        {
            // Set target for the next day
            targetTime = targetTime.AddHours(24);

            // Remove seconds and milliseconds to avoid stacking delays
            targetTime = targetTime.AddSeconds(-targetTime.Second).AddMilliseconds(-targetTime.Millisecond);
        }

        return targetTime;
    }

    public string GetTimeToNextFetchString()
    {
        // Get the time to the next fetch in a string format
        DateTime nextFetchTime = GetNextFetchDateTime();
        var target = nextFetchTime - DateTime.Now;

        var days = target.Days > 0 ? target.Days + "d " : "";
        var hours = target.Hours > 0 ? target.Hours + "h " : "";
        var minutes = target.Minutes > 0 ? target.Minutes + "m " : "";
        var seconds = target.Seconds > 0 ? target.Seconds + "s" : "";

        return $"{days}{hours}{minutes}{seconds}";
    }

    private async void ExecuteFetchingTask(object state)
    {
        // Check if Data Operations are required
        if (ReadyToClearPickedMovies()) // once a year
        {
            Logger.MainLog("== Clearing Picked Movies ==");
            DataService.Instance.ClearPickedMovies();
        }

        if (ReadyToClearUnpickedMovies()) // once a month
        {
            Logger.MainLog("== Clearing Unpicked Movies ==");
            DataService.Instance.ClearUnpickedMovies();
        }

        Logger.MainLog("== Fetching movies ==", false);

        var movies = await YouTubeService.GetMoviesAsync(Settings.Default.SuggestionString);

        if (movies != null)
        {
            // Attempt to get movie details from OMdb API
            var tasks = movies.Select(movie => OMdbService.GetMovieDetailsAsync(movie));
            await Task.WhenAll(tasks);

            // Add new movies to the list of movies
            DataService.Instance.AddMovies(movies);
        }
    }

    ///////////// Clearing /////////////
    private bool ReadyToClearUnpickedMovies()
    {
        return DateTime.Now >= GetNextClearUnpickedMoviesTime();
    }

    private bool ReadyToClearPickedMovies()
    {
        return DateTime.Now >= GetNextClearPickedMoviesTime();
    }

    private DateTime GetNextClearUnpickedMoviesTime()
    {
        // check if due per settings, then ensure it happens at 12am on the first day
        var nextClearTime = Settings.Default.LastClearUnpickedMoviesDateTime.AddMonths(1);

        Logger.MainLog($"Next Clear Unpicked Movies: {nextClearTime.ToString("F")}");

        return new DateTime(nextClearTime.Year, nextClearTime.Month, 1, 0, 0, 0);
    }

    private DateTime GetNextClearPickedMoviesTime()
    {
        // check if due per settings, then ensure it happens at 12am on the first day
        var nextClearTime = Settings.Default.LastClearPickedMoviesDateTime.AddYears(1);

        Logger.MainLog($"Next Clear   Picked Movies: {nextClearTime.ToString("F")}");

        return new DateTime(nextClearTime.Year, 1, 1, 0, 0, 0);
    }
}
