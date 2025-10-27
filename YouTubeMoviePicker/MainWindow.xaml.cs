using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using YouTubeMoviePicker.Controls;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Models.Extensions;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Services;

namespace YouTubeMoviePicker;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeTrayIcon();
        SetSettings(); 

        if (Settings.Default.StartMinimized)
        {
            WindowState = WindowState.Minimized;
            Hide();
        }

        // put the window back where it was
        Width = Settings.Default.WindowWidth;
        Height = Settings.Default.WindowHeight;
        Left = Settings.Default.WindowX;
        Top = Settings.Default.WindowY;

        DataService.Instance.DataChanged += (sender,e) => LoadMovies();

        LoadMovies();
    }


    // Initialization
    private async Task LoadMovies()
    {
        if (MovieList.Dispatcher.CheckAccess())
        {
            ClearMovies();
            DataService.Instance.LoadPosters();
            DataService.Instance.LoadData();
            var index = 1;

            if (DataService.Instance.UnpickedMoviesList.Count > 0)
            {
                foreach (var movie in DataService.Instance.UnpickedMoviesList)
                {
                    // Create MovieListItem and add it to the UI immediately
                    var movieListItem = new MovieListItem(movie, index.ToString());
                    index++;

                    // add context menu
                    AddContextMenuToMovieListItem(movieListItem);

                    // Add the MovieListItem to the MovieList UI container
                    MovieList.Children.Add(movieListItem);

                    // Register event handler for item click
                    movieListItem.ItemClicked += MovieListItem_ItemClicked;
                }

                SetSelectedMovie(DataService.Instance.UnpickedMoviesList[0]);
            }
            else
            {
                MovieList.Children.Add(new EmptyMovieListItem()); // quick start guide
                ClearSelectedMovie(); 
            }

            if (Settings.Default.EnableAutonomousDeployment)
                StartTimeToPostLabelTimer();
            else
            {
                IsMonitoringPostTime = false;
                TimeToPostLabel.FontSize = 12;
                TimeToPostLabel.Content = "AutoPost Disabled";
            }
        }
        else
        {
            // Recurs on the UI thread if not on the correct thread
            MovieList.Dispatcher.Invoke(() => LoadMovies());
        }
    }

    private void AddContextMenuToMovieListItem(MovieListItem movieListItem)
    {
        var contextMenu = new ContextMenu();

        var removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.ToolTip = "Send this movie to picked movie list";
        removeMenuItem.Click += (s, e) => DataService.Instance.PickMovie(movieListItem.Movie);

        var moveToBackMenuItem = new MenuItem { Header = "Send to Rear" };
        moveToBackMenuItem.ToolTip = "Send this movie to the back of the list";
        moveToBackMenuItem.Click += (s, e) => DataService.Instance.DeliverToRear(movieListItem.Movie);

        contextMenu.Items.Add(removeMenuItem);
        contextMenu.Items.Add(moveToBackMenuItem);

        movieListItem.ContextMenu = contextMenu;
    }

    private void ClearMovies()
    {
        if (MovieList.Dispatcher.CheckAccess())
        {
            // unsubscribe from events
            foreach (object Item in MovieList.Children)
            {
                if (Item is MovieListItem movieListItem)
                {
                    movieListItem.ItemClicked -= MovieListItem_ItemClicked;
                    
                    // remove context strip events
                    if (movieListItem.ContextMenu != null)
                    {
                        foreach (MenuItem menuItem in movieListItem.ContextMenu.Items)
                        {
                            menuItem.Click -= (s, e) => DataService.Instance.PickMovie(movieListItem.Movie);
                            menuItem.Click -= (s, e) => DataService.Instance.DeliverToRear(movieListItem.Movie);
                        }
                    }
                }
            }

            MovieList.Children.Clear();
        }
        else
        {
            MovieList.Dispatcher.Invoke(() => ClearMovies());
        }
    }

    private NotifyIcon TrayIcon;
    private void InitializeTrayIcon()
    {
        TrayIcon = new System.Windows.Forms.NotifyIcon();
        TrayIcon.BalloonTipIcon = ToolTipIcon.None;
        TrayIcon.BalloonTipTitle = "Many Movies, Such Free, Very Cool";
        TrayIcon.BalloonTipText = "Hey! I'm down here now!";
        TrayIcon.Icon = new System.Drawing.Icon("AppIcon.ico");

        TrayIcon.Text = "YouTube Movie Picker";

        StateChanged += OnStateChanged;
        TrayIcon.MouseDoubleClick += TrayIcon_DoubleClick;

        // Add a right-click menu to the tray icon
        TrayIcon.ContextMenuStrip = new ContextMenuStrip();
        TrayIcon.ContextMenuStrip.Items.Add("Open", null, TrayIcon_DoubleClick);
        TrayIcon.ContextMenuStrip.Items.Add("Exit", null, SystemTrayClose_Click);

        TrayIcon.Visible = true;
    }

    public void SetSettings()
    {
        if (Settings.Default.YouTubeApiKey == "")
        {
            FetchYouTubePageButton.Visibility = Visibility.Hidden;
            FetchCouterLabel.Visibility = Visibility.Hidden;
        }
        else
        {
            FetchYouTubePageButton.Visibility = Visibility.Visible;
            FetchCouterLabel.Visibility = Visibility.Visible;
        }

        if (Settings.Default.EnableDebug && Settings.Default.EnableDebugButtons)
        {
            ReloadOMbdDataButton.Visibility = Visibility.Visible;
        }
        else
        {
            ReloadOMbdDataButton.Visibility = Visibility.Hidden;
        }

        if (Settings.Default.EnablePostButton)
        {
            PostNowButton.Visibility = Visibility.Visible;
        }
        else
        {
            PostNowButton.Visibility = Visibility.Hidden;
        }

        if (Settings.Default.EnableAutonomousDeployment)
        {
            Scheduler.Instance.Start();
            StartTimeToPostLabelTimer();
        }
        else
        {
            IsMonitoringPostTime = false;
            TimeToPostLabel.Content = "AutoPost Disabled";
        }

        if (Settings.Default.LastManualFetchDateTime.Date != DateTime.Now.Date)
        {
            Settings.Default.ManualFetchCount = 0;
            FetchCouterLabel.Content = $"x{MaxFetches - Settings.Default.ManualFetchCount}";
            Settings.Default.Save();
        }

        FetchCouterLabel.Content = $"x{MaxFetches - Settings.Default.ManualFetchCount}";
        SuggestionBox.Text = Settings.Default.SuggestionString;
    }

    public bool IsMonitoringPostTime;
    public void StartTimeToPostLabelTimer()
    {
        // every second, update the time to post label
        IsMonitoringPostTime = false;
        Task.Run(async () =>
        {
            IsMonitoringPostTime = true;
            while (IsMonitoringPostTime)
            {
                var timeToPost = Scheduler.Instance.GetTimeToNextPostString();
                Dispatcher.Invoke(() => TimeToPostLabel.Content = "Next post in " + timeToPost);
                await Task.Delay(1000);
            }
        });
    }

    // Events
    private void MovieListItem_ItemClicked(object? sender, EventArgs e)
    {
        if (sender is MovieListItem movieListItem)
        {
            SetSelectedMovie(movieListItem.Movie);
        }
    }

    public Movie SelectedMovie { get; set; }
    public void SetSelectedMovie(Movie movie)
    {
        if (movie == null) return;
        
        SelectedMovie = movie;
        SelectedMovieTitle.Text = movie.Title;

        // if this movie is the next movie show it with header.
        var nextMovie = DataService.Instance.UnpickedMoviesList[0];
        if (nextMovie.YTVideoId == movie.YTVideoId)
            NextMovieHeaderLabel.Visibility = Visibility.Visible;
        else
            NextMovieHeaderLabel.Visibility = Visibility.Hidden;


        // OMdb details
        SelectedMovieGenre.Text = movie.Genre;
        SelectedMovieReleaseDate.Text = "Release Date: " + movie.Released;
        SelectedMovieDescription.Text = "    " + movie.Plot;
        SelectedMovieRating.Text = "Rated " + movie.Rated;
        SelectedMovieDuration.Text = "Runtime: " + movie.Runtime;
        SelectedMovieDirectors.Text = "Directors: " + movie.Director;
        SelectedMovieActors.Text = "Actors: " + movie.Actors;
        SelectedMovieWriters.Text = "Writers: " + movie.Writer;
        SelectedMovieBoxOffice.Text = "Box Office: " + movie.BoxOffice;
        SelectedMovieProduction.Text = "Production: " + movie.Production;
        SelectedMovieMetascore.Text = "Metascore: " + movie.Metascore + "/100";
        SelectedMovieImdbRating.Text = "IMDb Rating: " + movie.imdbRating + "/10  - Votes " + movie.imdbVotes;

        // youtube details
        SelectedMovieYTDescription.Text = $"'{movie.YTDescription}'";
        SelectedMovieChannelTitle.Text = "Posted by: " + movie.YTChannelTitle;
        SelectedMovieYTVideoPublishedAt.Text = "Posted on: " + movie.YTVideoPublishedAt;
        SelectedMovieYTVideoId.Text = $"Youtube Video Id: " + movie.YTVideoId;
        SelectedMovieYTVideoURL.NavigateUri = new Uri(movie.YTVideoURL);

        if (DataService.Instance.MoviePosters.Count == 0) DataService.Instance.LoadPosters();
        var poster = DataService.Instance.MoviePosters.FirstOrDefault(p => p.YTVideoid == movie.YTVideoId);
        if (poster != null)
        {
            SelectedMovieImage.Source = poster.Image;
        }
        else
        {
            try
            {


            }
            catch
            {
                SelectedMovieImage.Source = new BitmapImage(new Uri("/Resources/MissingImage.png", UriKind.Relative));
            }
        }
    }

    public void ClearSelectedMovie()
    {
        SelectedMovie = null;
        SelectedMovieTitle.Text = "No Movie Selected";
        SelectedMovieGenre.Text = "";
        SelectedMovieReleaseDate.Text = "";
        SelectedMovieDescription.Text = "";
        SelectedMovieRating.Text = "";
        SelectedMovieDuration.Text = "";
        SelectedMovieDirectors.Text = "";
        SelectedMovieActors.Text = "";
        SelectedMovieWriters.Text = "";
        SelectedMovieImage.Source = new BitmapImage(new Uri("/Resources/MissingImage.png", UriKind.Relative));
    }

    // Suggestion Box Events
    private Timer SearchTimer;
    private void SuggestionBox_TextChanged(object sender, RoutedEventArgs e)
    {
        // Stop the previous timer
        SearchTimer?.Stop();

        if (SuggestionBox.Text == "")
            SuggestionLabel.Visibility = Visibility.Visible;
        else
            SuggestionLabel.Visibility = Visibility.Hidden;

        // Start a new timer
        SearchTimer = new Timer();
        SearchTimer.Interval = 750;
        SearchTimer.Tick += SearchTimer_Elapsed;
        SearchTimer.Start();
        
        // For Effect
        LoadingSearchCanvas.Visibility = Visibility.Visible;
    }

    private void SearchTimer_Elapsed(object sender, EventArgs e)
    {
        // Perform the search after the set time of inactivity
        SearchTimer.Stop();
        SearchTimer = null;

        // remove Effect
        LoadingSearchCanvas.Visibility = Visibility.Hidden;

        var suggestionString = SuggestionBox.Text.Trim();
        var movies = DataService.Instance.UnpickedMoviesList;

        if (suggestionString == "") 
        {
            Settings.Default.SuggestionString = "";
            Settings.Default.NextPageToken = null;
            Settings.Default.Save();
        }

        if (suggestionString != null && suggestionString != Settings.Default.SuggestionString)
        {
            Settings.Default.SuggestionString = suggestionString;
            Settings.Default.SortBy = "Relevance";
            Settings.Default.NextPageToken = null;
            Settings.Default.Save();

            DataService.Instance.SortMoviesByRelevance(suggestionString);
        }

        //LoadMovies();
    }

    private void SettingsIcon_Clicked(object sender, RoutedEventArgs e)
    {
        SettingsMenu.ShowSettings();
    }

    // BUTTONS
    public async void PostNowButton_Click(object sender, RoutedEventArgs e)
    {
        // Check if a movie is selected
        if (SelectedMovie == null)
        {
            System.Windows.MessageBox.Show("No movie selected!", "No Movie", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Stop if all services are disabled
        if (!Settings.Default.SendToDiscord && !Settings.Default.SendToSlack && !Settings.Default.SendToTeams)
            return;

        var discordResult = "Failure"; // Default to failure
        var slackResult = "Failure";   // Default to failure
        var teamsResult = "Failure";   // Default to failure

        try
        {
            // Try to send the movie to Discord
            if (Settings.Default.SendToDiscord && !string.IsNullOrEmpty(Settings.Default.DiscordWebHook))
            {
                discordResult = await DiscordService.SendPayloadAsync(SelectedMovie.ToDiscordPayload());
            }
        }
        catch (Exception ex) 
        {
            Logger.MainLog("Error sending to Discord: " + ex.Message);
        }


        try
        {
            // Try to send the movie to Slack
            if (Settings.Default.SendToSlack && !string.IsNullOrEmpty(Settings.Default.SlackWebHook))
            {
                slackResult = await SlackService.SendPayloadAsync(SelectedMovie.ToSlackPayload(), SelectedMovie.Title, SelectedMovie.YTVideoURL);
            }
        }
        catch (Exception ex)
        {
            Logger.MainLog("Error sending to Slack: " + ex.Message);

        }


        try
        {
            // try to send movie to Teams
            if (Settings.Default.SendToTeams && !string.IsNullOrEmpty(Settings.Default.TeamsWebHook))
            {
                teamsResult = await TeamsService.SendPayloadAsync(SelectedMovie.ToTeamsPayload());
            }
        }
        catch (Exception ex)
        {
            Logger.MainLog("Error sending to Teams: " + ex.Message);
        }


        // Check if any results are successful
        if (slackResult == "Successful" || discordResult == "Successful" || teamsResult == "Successful")
        {
            // Move the movie to the Picked Movies list
            DataService.Instance.PickMovie(SelectedMovie);
        }
        else
        {
            Logger.MainLog($"Failed to post movie to Any services.\n==Full Mvie Details==\n{SelectedMovie.ToString()}");
            System.Windows.MessageBox.Show("Failed to post the movie. Please check your webhook settings.", "Post Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void FetchYouTubePageButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CheckFetchCount()) return;
        FetchYouTubePageButton.IsEnabled = false; // prevent multi clicks
        MakeThatShitSpin();

        DataService.Instance.LoadData();

        var movies = await YouTubeService.GetMoviesAsync(Settings.Default.SuggestionString);

        if (movies != null) // limit fetches
        {
            // Attempt to get movie details from OMdb API
            foreach (var movie in movies)
            {
                await OMdbService.GetMovieDetailsAsync(movie);
            }

            // Add new movies to the list of movies
            DataService.Instance.AddMovies(movies);
        }

        IncrementFetchCount();
        DataService.Instance.SortMoviesByRelevance(Settings.Default.SuggestionString); 
        
        KeepRollinRollinRollin = false;
        FetchYouTubePageButton.IsEnabled = true;
    }

    // Fetching 
    public int MaxFetches = 5;
    private bool CheckFetchCount()
    {
        var lastManualFetch = Settings.Default.LastManualFetchDateTime;

        if (lastManualFetch.Date != DateTime.Now.Date)
        {
            ResetFetchCount();
        }

        if (Settings.Default.ManualFetchCount >= MaxFetches)
        {
            ShowFetchLimitReachedMessage();
            return false;
        }

        return true;
    }

    private void ResetFetchCount()
    {
        Settings.Default.ManualFetchCount = 0;
        Settings.Default.LastManualFetchDateTime = DateTime.Now;
        Settings.Default.Save();
    }

    private void ShowFetchLimitReachedMessage()
    {
        System.Windows.MessageBox.Show("You have reached the maximum number of fetches for today. Please try again tomorrow.\n This limit keeps our services free!", "Fetch Limit Reached", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void IncrementFetchCount()
    {
        Settings.Default.ManualFetchCount++;
        Settings.Default.Save();
        FetchCouterLabel.Content = $"x{MaxFetches - Settings.Default.ManualFetchCount}";
    }

    private async void ReloadOMbdDataButton_Click(object sender, RoutedEventArgs e)
    {
        DataService.Instance.LoadData();
        foreach (var movie in DataService.Instance.UnpickedMoviesList)
        {
            await OMdbService.GetMovieDetailsAsync(movie);
        }

        DataService.Instance.SaveData();
        LoadMovies();
    }

    private async void SortBestRatings_Click(object sender, RoutedEventArgs e)
    {
        LoadingSearchCanvas.Visibility = Visibility.Visible;
        await Task.Delay(10);
        DataService.Instance.SortMoviesByRating();
        LoadingSearchCanvas.Visibility = Visibility.Hidden;
    }

    private async void SortByRelevance_Click(object sender, RoutedEventArgs e)
    {
        LoadingSearchCanvas.Visibility = Visibility.Visible;
        await Task.Delay(10);
        DataService.Instance.SortMoviesByRelevance(Settings.Default.SuggestionString);
        LoadingSearchCanvas.Visibility = Visibility.Hidden;
    }

    private async void SortRandom_Click(object sender, RoutedEventArgs e)
    {
        LoadingSearchCanvas.Visibility = Visibility.Visible;
        await Task.Delay(10);
        DataService.Instance.SortMoviesRandomly();
        LoadingSearchCanvas.Visibility = Visibility.Hidden;
    }

    // Animations
    public bool KeepRollinRollinRollin;
    private void MakeThatShitSpin()
    {
        var rotateTransform = new RotateTransform();
        FetchYouTubePageButton.RenderTransform = rotateTransform;
        FetchYouTubePageButton.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromSeconds(1),
            RepeatBehavior = RepeatBehavior.Forever
        };

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        KeepRollinRollinRollin = true;
        Task.Run(async () =>
        {
            while (KeepRollinRollinRollin)
            {
                await Task.Delay(100);
            }

            Dispatcher.Invoke(() =>
            {
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            });
        });
    }

    // System Tray Events
    public WindowState StoredWindowState = WindowState.Normal;
    public void OnStateChanged(object sender, EventArgs args)
    {
        if (WindowState == WindowState.Minimized)
        {
            // Reduce idle memory usage
            ClearMovies();
            DataService.Instance.ClearCache();

            Hide();
            TrayIcon?.ShowBalloonTip(2000);

            GC.Collect(); // Force garbage collection
        }
        else
        {
            // Reinitialize the main window content
            InitializeComponent();
            SetSettings();
            LoadMovies();

            //LoadMovies();
            DataService.Instance.LoadPosters();
            DataService.Instance.LoadData();
            StoredWindowState = WindowState;
        }
    }

    private void SystemTrayClose_Click(object sender, EventArgs e)
    {
        Close();
    }

    public void TrayIcon_DoubleClick(object sender, EventArgs e)
    {
        Show();
        WindowState = StoredWindowState;
    }

    // General Events
    public void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
        e.Handled = true;
    }

    // Confirm exit, Clean up, and Save settings
    private void OnExit(object sender, CancelEventArgs e)
    {
        MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure? If you close this program, there will be no glorious Auto-Posted Jackie Chan movies..", "What? why?", MessageBoxButton.OKCancel);

        if (result == MessageBoxResult.OK)
        {
            TrayIcon.Dispose();
            TrayIcon = null;

            // Save the window size and location
            Settings.Default.WindowWidth = (int)Width;
            Settings.Default.WindowHeight = (int)Height;
            Settings.Default.WindowX = (int)Left;
            Settings.Default.WindowY = (int)Top;
            Settings.Default.Save();
        }
        else
            e.Cancel = true;
    }
}