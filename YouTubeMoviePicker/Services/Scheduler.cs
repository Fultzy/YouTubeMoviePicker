using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Models.Extensions;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services
{
    internal sealed class Scheduler : IDisposable
    {
        // Thread-safe lazy singleton
        private static readonly Lazy<Scheduler> _lazyInstance = new Lazy<Scheduler>(() => new Scheduler());
        public static Scheduler Instance => _lazyInstance.Value;

        // Timers
        private Timer postingTimer;
        private Timer fetchingTimer;
        private Timer clearUnpickedTimer;
        private Timer clearPickedTimer;

        // For serializing executions so multiple timer callbacks don't run concurrently
        private readonly SemaphoreSlim executionLock = new SemaphoreSlim(1, 1);

        // For disposal
        private bool disposed;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private Scheduler() { }

        public void Start()
        {
            // Ensure stop
            StopAllTimers();

            Logger.MainLog(" == Starting Scheduler ==", false);

            // Schedule tasks
            SchedulePostingTask();
            ScheduleFetchingTask();

            // Schedule clean up
            ScheduleClearUnpickedTimer();
            ScheduleClearPickedTimer();
        }

        private void StopAllTimers()
        {
            postingTimer?.Dispose(); postingTimer = null;
            fetchingTimer?.Dispose(); fetchingTimer = null;
            clearUnpickedTimer?.Dispose(); clearUnpickedTimer = null;
            clearPickedTimer?.Dispose(); clearPickedTimer = null;
        }

        #region Posting

        private void SchedulePostingTask()
        {
            DateTime nextPostTime = GetNextPostDateTime();
            TimeSpan initialDelay = ClampToNonNegative(nextPostTime - DateTime.Now);

            Logger.MainLog($"Next movie  post in: {GetTimeToNextPostString()} => Targeting {nextPostTime:F}");
            postingTimer?.Dispose();

            // Timer will call wrapper which runs async Task safely and catches/logs exceptions.
            postingTimer = new Timer(_ => Task.Run(async () =>
            {
                try
                {
                    // use semaphore to prevent overlapping posting/fetching runs
                    if (await executionLock.WaitAsync(0))
                    {
                        try
                        {
                            await ExecutePostingTaskAsync(cts.Token);
                            SchedulePostingTask();
                        }
                        finally
                        {
                            executionLock.Release();
                        }
                    }
                    else
                    {
                        Logger.MainLog("Skipping posting run because another scheduler operation is running.");
                        SchedulePostingTask();
                    }
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"PostingTask error: {ex}"); 
                    SchedulePostingTask();
                }
            }, cts.Token), null, initialDelay, TimeSpan.FromDays(1)); // repeat daily
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

        private async Task ExecutePostingTaskAsync(CancellationToken token)
        {
            Logger.MainLog("== Posting Movie ==", false);

            // Stop if no movies to pick from
            if (DataService.Instance?.UnpickedMoviesList == null || DataService.Instance.UnpickedMoviesList.Count == 0)
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

            Movie nextMovie;
            try
            {
                // take a snapshot to reduce time under any external locking
                nextMovie = DataService.Instance.UnpickedMoviesList[0];
            }
            catch (Exception ex)
            {
                Logger.MainLog($"Failed to get next movie: {ex}");
                return;
            }

            var discordResult = "Failure";
            var slackResult = "Failure";
            var teamsResult = "Failure";

            try
            {
                if (Settings.Default.SendToDiscord && !string.IsNullOrEmpty(Settings.Default.DiscordWebHook))
                {
                    // Assume DiscordService returns a string like "Successful" or "Failure"
                    discordResult = await DiscordService.SendPayloadAsync(nextMovie.ToDiscordPayload());
                }
            }
            catch (Exception ex)
            {
                Logger.MainLog($"Discord send error: {ex}");
            }

            try
            {
                if (Settings.Default.SendToSlack && !string.IsNullOrEmpty(Settings.Default.SlackWebHook))
                {
                    slackResult = await SlackService.SendPayloadAsync(nextMovie.ToSlackPayload(), nextMovie.Title, nextMovie.YTVideoURL);
                }
            }
            catch (Exception ex)
            {
                Logger.MainLog($"Slack send error: {ex}");
            }

            try
            {
                if (Settings.Default.SendToTeams && !string.IsNullOrEmpty(Settings.Default.TeamsWebHook))
                {
                    teamsResult = await TeamsService.SendPayloadAsync(nextMovie.ToTeamsPayload());
                }
            }
            catch (Exception ex)
            {
                Logger.MainLog($"Teams send error: {ex}");
            }


            // If either succeeded, mark the movie as picked
            if (slackResult == "Successful" || discordResult == "Successful" || teamsResult == "Successful")
            {
                var msg1 = slackResult == "Successful" ? " Slack" : "";
                var msg2 = discordResult == "Successful" ? " Discord" : "";
                var msg3 = teamsResult == "Successful" ? " Teams" : "";
                Logger.MainLog($"Posted {nextMovie.Title} to{msg1}{msg2}{msg3} Successfully!");

                try
                {
                    DataService.Instance.PickNextMovie();
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"Failed to move movie to picked list: {ex}");
                }
            }
            else
            {
                Logger.MainLog($"Posting failed for {nextMovie.Title}. Discord: {discordResult}, Slack: {slackResult}");
            }
        }

        private DateTime GetNextPostDateTime()
        {
            // index math (hour +1, minute * 30)
            var hour = Settings.Default.HourToPostIndex + 1;
            var hourAdj = Settings.Default.AmPmToPostIndex == 1 ? 12 : 0;
            var minute = Settings.Default.MinuteToPostIndex * 30;

            DateTime targetTime = DateTime.Today.AddHours(hour + hourAdj).AddMinutes(minute);

            // Normalize seconds/milliseconds
            targetTime = targetTime.AddSeconds(-targetTime.Second).AddMilliseconds(-targetTime.Millisecond);

            // If the post time has already passed for today, advance to the next day
            if (DateTime.Now > targetTime)
            {
                targetTime = targetTime.AddDays(1);
            }

            // Use targetTime.DayOfWeek so we consider the correct day after we moved it
            int currentDayIndex = (int)targetTime.DayOfWeek; // Sunday = 0 ... Saturday = 6

            var daysPattern = Settings.Default.DaysToRun ?? "XXXXXXX";
            if (daysPattern.Length != 7)
            {
                Logger.MainLog("Warning: DaysToRun setting is not 7 characters. Defaulting to no days enabled.");
                daysPattern = "XXXXXXX";
            }

            // Advance targetTime until we find an 'A'
            int guard = 0;
            while (daysPattern[currentDayIndex] != 'A' && guard++ < 8)
            {
                targetTime = targetTime.AddDays(1);
                currentDayIndex = (currentDayIndex + 1) % 7;
            }

            return targetTime;
        }

        #endregion

        #region Fetching

        private void ScheduleFetchingTask()
        {
            DateTime nextFetchTime = GetNextFetchDateTime();
            TimeSpan initialDelay = ClampToNonNegative(nextFetchTime - DateTime.Now);

            Logger.MainLog($"Next movie fetch in: {GetTimeToNextFetchString()} => Targeting {nextFetchTime:F}");
            fetchingTimer?.Dispose();

            fetchingTimer = new Timer(_ => Task.Run(async () =>
            {
                try
                {
                    if (await executionLock.WaitAsync(0))
                    {
                        try
                        {
                            await ExecuteFetchingTaskAsync(cts.Token);
                            ScheduleFetchingTask();
                        }
                        finally
                        {
                            executionLock.Release();
                        }
                    }
                    else
                    {
                        Logger.MainLog("Skipping fetching run because another scheduler operation is running.");
                        ScheduleFetchingTask();
                    }
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"FetchingTask error: {ex}");
                    ScheduleFetchingTask();
                }
            }, cts.Token), null, initialDelay, TimeSpan.FromDays(1)); // daily fetch repetition
        }

        private DateTime GetNextFetchDateTime()
        {
            var hour = Settings.Default.HourToFetchIndex + 1;
            var hourAdj = Settings.Default.AmPmToFetchIndex == 1 ? 12 : 0;
            var minute = Settings.Default.MinuteToFetchIndex * 30;
            DateTime targetTime = DateTime.Today.AddHours(hour + hourAdj).AddMinutes(minute);

            // Normalize seconds/milliseconds
            targetTime = targetTime.AddSeconds(-targetTime.Second).AddMilliseconds(-targetTime.Millisecond);

            if (DateTime.Now > targetTime)
            {
                targetTime = targetTime.AddDays(1);
            }

            return targetTime;
        }

        public string GetTimeToNextFetchString()
        {
            DateTime nextFetchTime = GetNextFetchDateTime();
            var target = nextFetchTime - DateTime.Now;

            var days = target.Days > 0 ? target.Days + "d " : "";
            var hours = target.Hours > 0 ? target.Hours + "h " : "";
            var minutes = target.Minutes > 0 ? target.Minutes + "m " : "";
            var seconds = target.Seconds > 0 ? target.Seconds + "s" : "";

            return $"{days}{hours}{minutes}{seconds}";
        }

        private async Task ExecuteFetchingTaskAsync(CancellationToken token)
        {
            try
            {
                if (ReadyToClearPickedMovies())
                {
                    Logger.MainLog("== Clearing Picked Movies ==");
                    DataService.Instance.ClearPickedMovies();
                }

                if (ReadyToClearUnpickedMovies())
                {
                    Logger.MainLog("== Clearing Unpicked Movies ==");
                    DataService.Instance.ClearUnpickedMovies();
                    await ExecuteFetchingTaskAsync(cts.Token);
                }
            }
            catch (Exception ex)
            {
                Logger.MainLog($"Error during clearing: {ex}");
            }

            Logger.MainLog("== Fetching movies ==", false);

            var movies = await YouTubeService.GetMoviesAsync(Settings.Default.SuggestionString);

            if (movies != null && movies.Any())
            {
                try
                {
                    var tasks = movies.Select(movie => OMdbService.GetMovieDetailsAsync(movie));
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"Error fetching OMDb details: {ex}");
                }

                try
                {
                    DataService.Instance.AddMovies(movies);
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"Error adding movies to DataService: {ex}");
                }
            }
        }

        #endregion

        #region Clearing timers (reschedule after run)

        private void ScheduleClearUnpickedTimer()
        {
            DateTime next = GetNextClearUnpickedMoviesTime();
            TimeSpan initialDelay = ClampToNonNegative(next - DateTime.Now);

            clearUnpickedTimer?.Dispose();
            clearUnpickedTimer = new Timer(async _ =>
            {
                try
                {
                    if (await executionLock.WaitAsync(0))
                    {
                        try
                        {
                            Logger.MainLog("== Timer triggered: Clearing Unpicked Movies ==");
                            DataService.Instance.ClearUnpickedMovies();
                            // After clearing, reschedule the timer to the next monthly run
                            ScheduleClearUnpickedTimer();
                        }
                        finally
                        {
                            executionLock.Release();
                        }
                    }
                    else
                    {
                        Logger.MainLog("Skipping clear unpicked run because another scheduler operation is running.");
                        // Reschedule anyway so it triggers next time correctly
                        ScheduleClearUnpickedTimer();
                    }
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"ClearUnpickedTask error: {ex}");
                    ScheduleClearUnpickedTimer();
                }
            }, null, initialDelay, Timeout.InfiniteTimeSpan);
        }

        private static readonly TimeSpan MaxTimerInterval = TimeSpan.FromMilliseconds((double)uint.MaxValue - 2);

        private void ScheduleClearPickedTimer()
        {
            DateTime target = GetNextClearPickedMoviesTime();
            TimeSpan initialDelay = ClampToNonNegative(target - DateTime.Now);

            clearPickedTimer?.Dispose();
            TimeSpan due = initialDelay > MaxTimerInterval ? MaxTimerInterval : initialDelay;

            clearPickedTimer = new Timer(async _ =>
            {
                try
                {
                    // If fired early because we capped due, re-evaluate whether it's time yet
                    if (DateTime.Now < target)
                    {
                        ScheduleClearPickedTimer();
                        return;
                    }

                    if (await executionLock.WaitAsync(0))
                    {
                        try
                        {
                            Logger.MainLog("== Timer triggered: Clearing Picked Movies ==");
                            DataService.Instance.ClearPickedMovies();
                            ScheduleClearPickedTimer(); // schedule next yearly run
                        }
                        finally
                        {
                            executionLock.Release();
                        }
                    }
                    else
                    {
                        Logger.MainLog("Skipping clear picked run because another scheduler operation is running.");
                        ScheduleClearPickedTimer();
                    }
                }
                catch (Exception ex)
                {
                    Logger.MainLog($"ClearPickedTask error: {ex}");
                    ScheduleClearPickedTimer();
                }
            }, null, due, Timeout.InfiniteTimeSpan);
        }

        private bool ReadyToClearUnpickedMovies() => DateTime.Now >= GetNextClearUnpickedMoviesTime();
        private bool ReadyToClearPickedMovies() => DateTime.Now >= GetNextClearPickedMoviesTime();

        private DateTime GetNextClearUnpickedMoviesTime()
        {
            // Next = last clear + 1 month, but execute at 12:00 AM on day 1 of that month
            var nextClearTime = Settings.Default.LastClearUnpickedMoviesDateTime.AddMonths(1);

            Logger.MainLog($"Next Clear Unpicked Movies (raw): {nextClearTime:F}");
            return new DateTime(nextClearTime.Year, nextClearTime.Month, 1, 0, 0, 0);
        }

        private DateTime GetNextClearPickedMoviesTime()
        {
            // Next = last clear + 1 year, but execute at 12:00 AM Jan 1 of that year
            var nextClearTime = Settings.Default.LastClearPickedMoviesDateTime.AddYears(1);

            Logger.MainLog($"Next Clear   Picked Movies (raw): {nextClearTime:F}");
            return new DateTime(nextClearTime.Year, 1, 1, 0, 0, 0);
        }

        #endregion

        #region Helpers & IDisposable

        private static TimeSpan ClampToNonNegative(TimeSpan ts) => ts < TimeSpan.Zero ? TimeSpan.Zero : ts;

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            try
            {
                cts.Cancel();
            }
            catch { /* ignore */ }

            StopAllTimers();
            executionLock.Dispose();
            cts.Dispose();
        }

        #endregion
    }
}












// OLD SYSTEM

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Media.Animation;
//using YouTubeMoviePicker.Models;
//using YouTubeMoviePicker.Models.Extensions;
//using YouTubeMoviePicker.Properties;
//using YouTubeMoviePicker.Utility;

//namespace YouTubeMoviePicker.Services;
//internal class Scheduler
//{
//    private static Scheduler _instance;
//    public static Scheduler Instance
//    {
//        get
//        {
//            if (_instance == null)
//            {
//                _instance = new Scheduler();
//            }
//            return _instance;
//        }
//    }

//    private Timer postingTimer;
//    private Timer fetchingTimer;
//    private Timer clearUnpickedTimer;
//    private Timer clearPickedTimer;

//    public void Start()
//    {
//        // Ensure we stop any active timers before starting new ones
//        StopAllTimers();

//        // Schedule each task with the current settings
//        Logger.MainLog(" == Starting Scheduler ==", false);

//        SchedulePostingTask();
//        ScheduleFetchingTask(); // Data Operations have been moved here

//        var test = ReadyToClearUnpickedMovies();
//        var test2 = ReadyToClearPickedMovies();
//    }

//    private void StopAllTimers()
//    {
//        postingTimer?.Dispose();
//        fetchingTimer?.Dispose();
//        clearUnpickedTimer?.Dispose();
//        clearPickedTimer?.Dispose();
//    }

//    ///////////// Posting /////////////
//    private void SchedulePostingTask()
//    {
//        DateTime nextPostTime = GetNextPostDateTime();
//        TimeSpan initialDelay = nextPostTime - DateTime.Now;

//        Logger.MainLog($"Next movie  post in: {GetTimeToNextPostString()} => Targeting {nextPostTime.ToString("F")}");
//        postingTimer = new Timer(ExecutePostingTask, null, initialDelay, TimeSpan.FromDays(1));  // Post daily
//    }

//    public string GetTimeToNextPostString()
//    {
//        DateTime nextPostTime = GetNextPostDateTime();
//        var target = nextPostTime - DateTime.Now;

//        var days = target.Days > 0 ? target.Days + "d " : "";
//        var hours = target.Hours > 0 ? target.Hours + "h " : "";
//        var minutes = target.Minutes > 0 ? target.Minutes + "m " : "";
//        var seconds = target.Seconds > 0 ? target.Seconds + "s" : ""; 

//        return $"{days}{hours}{minutes}{seconds}";
//    }

//    private async void ExecutePostingTask(object state)
//    {
//        Logger.MainLog("== Posting Movie ==", false);

//        // Stop if no movies to pick from
//        if (DataService.Instance.UnpickedMoviesList.Count == 0)
//        {
//            Logger.MainLog("No Movies to post!!");
//            return;
//        }

//        // Stop if both WebHooks are disabled
//        if (!Settings.Default.SendToDiscord && !Settings.Default.SendToSlack)
//        {
//            Logger.MainLog("No WebHooks enabled!!");
//            return;
//        }

//        // select next movie in list
//        var nextMovie = DataService.Instance.UnpickedMoviesList[0];

//        var discordResult = "Failure"; // Default to failure
//        var slackResult = "Failure";   // Default to failure

//        // Try to send the movie to Discord
//        if (Settings.Default.SendToDiscord && !string.IsNullOrEmpty(Settings.Default.DiscordWebHook))
//        {
//            discordResult = await DiscordService.SendPayloadAsync(nextMovie.ToDiscordPayload());
//        }

//        // Try to send the movie to Slack
//        if (Settings.Default.SendToSlack && !string.IsNullOrEmpty(Settings.Default.SlackWebHook))
//        {
//            slackResult = await SlackService.SendPayloadAsync(nextMovie.ToSlackPayload(), nextMovie.Title, nextMovie.YTVideoURL);
//        }

//        // Check if any results are successful
//        if (slackResult == "Successful" || discordResult == "Successful")
//        {
//            var msg1 = slackResult == "Successful" ? " Slack" : "";
//            var msg2 = discordResult == "Successful" ? " Discord" : "";
//            Logger.MainLog($"Posted {nextMovie.Title} to{msg1}{msg2} Successfully!");

//            // Move the movie to the Picked Movies list
//            DataService.Instance.PickNextMovie();
//        }
//    }

//    private DateTime GetNextPostDateTime()
//    {
//        // Find the next valid day and time for posting
//        var hour = Settings.Default.HourToPostIndex + 1;
//        var hourAdj = Settings.Default.AmPmToPostIndex == 1 ? 12 : 0;
//        var minute = Settings.Default.MinuteToPostIndex * 30;
//        DateTime targetTime = DateTime.Today.AddHours(hour + hourAdj).AddMinutes(minute);

//        // Check if the post time has already passed
//        if (DateTime.Now > targetTime)
//        {
//            // Set target for the next day
//            targetTime = targetTime.AddHours(24);

//            // Remove any seconds and milliseconds to avoid stacking delays
//            targetTime = targetTime.AddSeconds(-targetTime.Second).AddMilliseconds(-targetTime.Millisecond);
//        }

//        // Get the current day of the week Index
//        int currentDayIndex = (int)DateTime.Now.DayOfWeek; // Sunday = 0, Saturday = 6

//        // Check if the current day matches the allowed posting days
//        // (SMTWHFA as XAAAAAX) 'A' represents available day
//        while (Settings.Default.DaysToRun[currentDayIndex] != 'A')  
//        {
//            // Adjust target day (increment until matching the next available day)
//            targetTime = targetTime.AddDays(1);
//            currentDayIndex = (currentDayIndex + 1) % 7;  // Loop through the days
//        }

//        return targetTime;
//    }

//    ///////////// Fetching /////////////
//    private void ScheduleFetchingTask()
//    {
//        // get the time to the next fetch
//        DateTime nextFetchTime = GetNextFetchDateTime();
//        TimeSpan initialDelay = nextFetchTime - DateTime.Now;

//        Logger.MainLog($"Next movie fetch in: {GetTimeToNextFetchString()} => Targeting {nextFetchTime.ToString("F")}");

//        fetchingTimer = new Timer(ExecuteFetchingTask, null, initialDelay, TimeSpan.FromDays(1));
//    }

//    private DateTime GetNextFetchDateTime()
//    {
//        var hour = Settings.Default.HourToFetchIndex + 1;
//        var hourAdj = Settings.Default.AmPmToFetchIndex == 1 ? 12 : 0;
//        var minute = Settings.Default.MinuteToFetchIndex * 30;
//        DateTime targetTime = DateTime.Today.AddHours(hour + hourAdj).AddMinutes(minute);

//        // Check if the fetch time has already passed
//        if (DateTime.Now > targetTime)
//        {
//            // Set target for the next day
//            targetTime = targetTime.AddHours(24);

//            // Remove seconds and milliseconds to avoid stacking delays
//            targetTime = targetTime.AddSeconds(-targetTime.Second).AddMilliseconds(-targetTime.Millisecond);
//        }

//        return targetTime;
//    }

//    public string GetTimeToNextFetchString()
//    {
//        // Get the time to the next fetch in a string format
//        DateTime nextFetchTime = GetNextFetchDateTime();
//        var target = nextFetchTime - DateTime.Now;

//        var days = target.Days > 0 ? target.Days + "d " : "";
//        var hours = target.Hours > 0 ? target.Hours + "h " : "";
//        var minutes = target.Minutes > 0 ? target.Minutes + "m " : "";
//        var seconds = target.Seconds > 0 ? target.Seconds + "s" : "";

//        return $"{days}{hours}{minutes}{seconds}";
//    }

//    private async void ExecuteFetchingTask(object state)
//    {
//        // Check if Data Operations are required
//        if (ReadyToClearPickedMovies()) // once a year
//        {
//            Logger.MainLog("== Clearing Picked Movies ==");
//            DataService.Instance.ClearPickedMovies();
//        }

//        if (ReadyToClearUnpickedMovies()) // once a month
//        {
//            Logger.MainLog("== Clearing Unpicked Movies ==");
//            DataService.Instance.ClearUnpickedMovies();
//        }

//        Logger.MainLog("== Fetching movies ==", false);

//        var movies = await YouTubeService.GetMoviesAsync(Settings.Default.SuggestionString);

//        if (movies != null)
//        {
//            // Attempt to get movie details from OMdb API
//            var tasks = movies.Select(movie => OMdbService.GetMovieDetailsAsync(movie));
//            await Task.WhenAll(tasks);

//            // Add new movies to the list of movies
//            DataService.Instance.AddMovies(movies);
//        }
//    }

//    ///////////// Clearing /////////////
//    private bool ReadyToClearUnpickedMovies()
//    {
//        return DateTime.Now >= GetNextClearUnpickedMoviesTime();
//    }

//    private bool ReadyToClearPickedMovies()
//    {
//        return DateTime.Now >= GetNextClearPickedMoviesTime();
//    }

//    private DateTime GetNextClearUnpickedMoviesTime()
//    {
//        // check if due per settings, then ensure it happens at 12am on the first day
//        var nextClearTime = Settings.Default.LastClearUnpickedMoviesDateTime.AddMonths(1);

//        Logger.MainLog($"Next Clear Unpicked Movies: {nextClearTime.ToString("F")}");

//        return new DateTime(nextClearTime.Year, nextClearTime.Month, 1, 0, 0, 0);
//    }

//    private DateTime GetNextClearPickedMoviesTime()
//    {
//        // check if due per settings, then ensure it happens at 12am on the first day
//        var nextClearTime = Settings.Default.LastClearPickedMoviesDateTime.AddYears(1);

//        Logger.MainLog($"Next Clear   Picked Movies: {nextClearTime.ToString("F")}");

//        return new DateTime(nextClearTime.Year, 1, 1, 0, 0, 0);
//    }
//}
