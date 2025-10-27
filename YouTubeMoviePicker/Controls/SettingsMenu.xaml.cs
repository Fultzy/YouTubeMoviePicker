using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Services;

namespace YouTubeMoviePicker.Controls;

/// <summary>
/// Interaction logic for SettingsMenu.xaml
/// </summary>
public partial class SettingsMenu : UserControl
{
    public SettingsMenu()
    {
        InitializeComponent();
    }

    public void ShowSettings()
    {
        InitializeSettings();
        this.Visibility = Visibility.Visible;

    }

    private void InitializeSettings()
    {
        // Api keys
        YouTubeApiKeyTextBox.Text = Properties.Settings.Default.YouTubeApiKey;
        SlackWebHookTextBox.Text = Properties.Settings.Default.SlackWebHook;
        TeamsWebHookTextBox.Text = Properties.Settings.Default.TeamsWebHook;
        OMdbApiKeyTextBox.Text = Properties.Settings.Default.OMdbApiKey;
        DiscordWebHookTextBox.Text = Properties.Settings.Default.DiscordWebHook;

        // Enable/Disable checkboxes
        if (Properties.Settings.Default.SendToDiscord) DiscordWebHookCheckBox.IsChecked = true;
        if (Properties.Settings.Default.SendToSlack) SlackWebHookCheckBox.IsChecked = true;
        if (Properties.Settings.Default.SendToTeams) TeamsWebHookCheckBox.IsChecked = true;
        if (Properties.Settings.Default.EnableAutonomousDeployment) EnableAutoPostingCheckBox.IsChecked = true;

        // Time to run
        HoursToRunComboBox.SelectedIndex = Properties.Settings.Default.HourToPostIndex;
        MinutesToRunComboBox.SelectedIndex = Properties.Settings.Default.MinuteToPostIndex;
        AmPmComboBox.SelectedIndex = Properties.Settings.Default.AmPmToPostIndex;
        CheckDaysToRun(Properties.Settings.Default.DaysToRun);

        // Time to fetch
        HoursToFetchComboBox.SelectedIndex = Properties.Settings.Default.HourToFetchIndex;
        MinutesToFetchComboBox.SelectedIndex = Properties.Settings.Default.MinuteToFetchIndex;
        AmPmFetchComboBox.SelectedIndex = Properties.Settings.Default.AmPmToFetchIndex;

        
        // General
        FullLengthPlotCheckbox.IsChecked = Properties.Settings.Default.EnableFullPlots;
        DebuggingCheckbox.IsChecked = Properties.Settings.Default.EnableDebug;
        DebuggingButtonsCheckbox.IsChecked = Properties.Settings.Default.EnableDebugButtons;
        VerboseLoggingCheckbox.IsChecked = Properties.Settings.Default.EnableVerboseLogs;
        PostNowCheckbox.IsChecked = Properties.Settings.Default.EnablePostButton;
        
        // Windows
        StartWithWindowsCheckbox.IsChecked = Properties.Settings.Default.StartWithWindows;
        StartMinimizedCheckbox.IsChecked = Properties.Settings.Default.StartMinimized;

        // Fields
        NextPageTokenTextBlock.Text = Properties.Settings.Default.NextPageToken;

        var pickedMsg = DataService.Instance.PickedMovieFileDetailsString();
        var unpickedMsg = DataService.Instance.UnpickedMovieFileDetailsString();

        UnpickedMovieTextBlock.Text = unpickedMsg;
        PickedMovieTextBlock.Text = pickedMsg;
    }

    private void SettingsSaveButton_Clicked(object sender, RoutedEventArgs e)
    {
        if (SettingsAreValid())
        {
            SaveSettings();

            // apply Debug settings to main
            var mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.SetSettings();

            this.Visibility = Visibility.Collapsed;
        }
    }

    private void SaveSettings()
    {
        // Api Keys
        Properties.Settings.Default.YouTubeApiKey = YouTubeApiKeyTextBox.Text;
        Properties.Settings.Default.SlackWebHook = SlackWebHookTextBox.Text;
        Properties.Settings.Default.TeamsWebHook = TeamsWebHookTextBox.Text;
        Properties.Settings.Default.OMdbApiKey = OMdbApiKeyTextBox.Text;
        Properties.Settings.Default.DiscordWebHook = DiscordWebHookTextBox.Text;

        // Enable/Disable checkboxes
        Properties.Settings.Default.SendToDiscord = DiscordWebHookCheckBox.IsChecked == true;
        Properties.Settings.Default.SendToSlack = SlackWebHookCheckBox.IsChecked == true;
        Properties.Settings.Default.SendToTeams = TeamsWebHookCheckBox.IsChecked == true;
        Properties.Settings.Default.EnableAutonomousDeployment = EnableAutoPostingCheckBox.IsChecked == true;

        // Time to Post
        Properties.Settings.Default.HourToPostIndex = HoursToRunComboBox.SelectedIndex;
        Properties.Settings.Default.MinuteToPostIndex = MinutesToRunComboBox.SelectedIndex;
        Properties.Settings.Default.AmPmToPostIndex = AmPmComboBox.SelectedIndex;
        Properties.Settings.Default.DaysToRun = DaysToRun;

        // Time to Fetch
        Properties.Settings.Default.HourToFetchIndex = HoursToFetchComboBox.SelectedIndex;
        Properties.Settings.Default.MinuteToFetchIndex = MinutesToFetchComboBox.SelectedIndex;
        Properties.Settings.Default.AmPmToFetchIndex = AmPmFetchComboBox.SelectedIndex;

        // General
        Properties.Settings.Default.EnableFullPlots = FullLengthPlotCheckbox.IsChecked == true;
        Properties.Settings.Default.EnableDebug = DebuggingCheckbox.IsChecked == true;
        Properties.Settings.Default.EnableDebugButtons = DebuggingButtonsCheckbox.IsChecked == true;
        Properties.Settings.Default.EnableVerboseLogs = VerboseLoggingCheckbox.IsChecked == true;
        Properties.Settings.Default.EnablePostButton = PostNowCheckbox.IsChecked == true;

        // Windows
        Properties.Settings.Default.StartWithWindows = StartWithWindowsCheckbox.IsChecked == true;
        Properties.Settings.Default.StartMinimized = StartMinimizedCheckbox.IsChecked == true;

        Properties.Settings.Default.Save();
    }

    private bool SettingsAreValid()
    {
        var ytApiResult = Services.Validator.IsValidYouTubeApiKey(YouTubeApiKeyTextBox.Text);
        if (ytApiResult != "pass")
        {
            MessageBox.Show(ytApiResult, "Invalid API Key", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var slackWebHookResult = Services.Validator.IsValidSlackWebHook(SlackWebHookTextBox.Text);
        if (SlackWebHookCheckBox.IsChecked == true && slackWebHookResult != "pass")
        {
            MessageBox.Show(slackWebHookResult, "Invalid WebHook", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var teamsWebHookResult = Services.Validator.IsValidTeamsWebHook(TeamsWebHookTextBox.Text);
        if (TeamsWebHookCheckBox.IsChecked == true && teamsWebHookResult != "pass")
        {
            MessageBox.Show(teamsWebHookResult, "Invalid WebHook", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var omdbApiResult = Services.Validator.IsValidOMdbApiKey(OMdbApiKeyTextBox.Text);
        if (omdbApiResult != "pass")
        {
            MessageBox.Show(omdbApiResult, "Invalid API Key", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var discordApiResult = Services.Validator.IsValidDiscordWebHook(DiscordWebHookTextBox.Text);
        if (DiscordWebHookCheckBox.IsChecked == true && discordApiResult != "pass")
        {
            MessageBox.Show(discordApiResult, "Invalid WebHook", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        
        return true;
    }

    private void SettingsCancelButton_Clicked(object sender, RoutedEventArgs e)
    {
        InitializeSettings();
        this.Visibility = Visibility.Collapsed;
    }

    public void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
        e.Handled = true;
    }

    // Text Events
    private void DiscordWebHookTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!DiscordWebHookCheckBox.IsChecked == true)
        {
            DiscordWebHookTextBox.Background = Brushes.White;
            return;
        }

        if (Services.Validator.IsValidDiscordWebHook(DiscordWebHookTextBox.Text) == "pass")
        {
            DiscordWebHookTextBox.Background = Brushes.LightGreen;
        }
        else
        {
            DiscordWebHookTextBox.Background = Brushes.LightCoral;
        }
    }

    private void SlackWebHookTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!SlackWebHookCheckBox.IsChecked == true)
        {
            SlackWebHookTextBox.Background = Brushes.White;
            return; 
        }
        if (Services.Validator.IsValidSlackWebHook(SlackWebHookTextBox.Text) == "pass")
        {
            SlackWebHookTextBox.Background = Brushes.LightGreen;
        }
        else
        {
            SlackWebHookTextBox.Background = Brushes.LightCoral;
        }
    }

    private void TeamsWebHookTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!TeamsWebHookCheckBox.IsChecked == true)
        {
            TeamsWebHookTextBox.Background = Brushes.White;
            return;
        }
        if (Services.Validator.IsValidTeamsWebHook(SlackWebHookTextBox.Text) == "pass")
        {
            TeamsWebHookTextBox.Background = Brushes.LightGreen;
        }
        else
        {
            TeamsWebHookTextBox.Background = Brushes.LightCoral;
        }
    }

    private void OMdbApiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Services.Validator.IsValidOMdbApiKey(OMdbApiKeyTextBox.Text) == "pass")
        {
            OMdbApiKeyTextBox.Background = Brushes.LightGreen;
        }
        else
        {
            OMdbApiKeyTextBox.Background = Brushes.LightCoral;
        }
    }

    private void YouTubeApiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Services.Validator.IsValidYouTubeApiKey(YouTubeApiKeyTextBox.Text) == "pass")
        {
            YouTubeApiKeyTextBox.Background = Brushes.LightGreen;
        }
        else
        {
            YouTubeApiKeyTextBox.Background = Brushes.LightCoral;
        }
    }

    // API Checkbox Events
    private void SlackCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        SlackWebHookTextBox.IsEnabled = true;
        SlackWebHookTextBox_TextChanged(sender, null);  
    }

    private void SlackCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        SlackWebHookTextBox.IsEnabled = false;
        SlackWebHookTextBox_TextChanged(sender, null);
    }
    private void TeamsCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        TeamsWebHookTextBox.IsEnabled = true;
        TeamsWebHookTextBox_TextChanged(sender, null);
    }

    private void TeamsCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        TeamsWebHookTextBox.IsEnabled = false;
        TeamsWebHookTextBox_TextChanged(sender, null);
    }

    private void DiscordCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        DiscordWebHookTextBox.IsEnabled = false;
        DiscordWebHookTextBox_TextChanged(sender, null);
    }

    private void DiscordCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        DiscordWebHookTextBox.IsEnabled = true;
        DiscordWebHookTextBox_TextChanged(sender, null);
    }

    // Day / Time Events
    public string DaysToRun { get; set; }
    private void UpdateDaysToRun(object sender, RoutedEventArgs e)
    {
        DaysToRun = "";
        DaysToRun += SundayCheckBox.IsChecked == true ? "A" : "X";
        DaysToRun += MondayCheckBox.IsChecked == true ? "A" : "X";
        DaysToRun += TuesdayCheckBox.IsChecked == true ? "A" : "X";
        DaysToRun += WednesdayCheckBox.IsChecked == true ? "A" : "X";
        DaysToRun += ThursdayCheckBox.IsChecked == true ? "A" : "X";
        DaysToRun += FridayCheckBox.IsChecked == true ? "A" : "X";
        DaysToRun += SaturdayCheckBox.IsChecked == true ? "A" : "X";
    }

    private void CheckDaysToRun(string daysToRun)
    {
        SundayCheckBox.IsChecked = daysToRun[0] == 'A';
        MondayCheckBox.IsChecked = daysToRun[1] == 'A';
        TuesdayCheckBox.IsChecked = daysToRun[2] == 'A';
        WednesdayCheckBox.IsChecked = daysToRun[3] == 'A';
        ThursdayCheckBox.IsChecked = daysToRun[4] == 'A';
        FridayCheckBox.IsChecked = daysToRun[5] == 'A';
        SaturdayCheckBox.IsChecked = daysToRun[6] == 'A';

        DaysToRun = daysToRun;
    }

    // Setting Checkbox Events
    private void DebuggingCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        DebuggingGroupBox.Visibility = Visibility.Visible;
        SettingsContentRow.MaxHeight = 860;
    }

    private void DebuggingCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        DebuggingGroupBox.Visibility = Visibility.Collapsed;
        SettingsContentRow.MaxHeight = 700;
    }

    // Click Events
    private void ClearPageTokenButton_Clicked(object sender, RoutedEventArgs e)
    {
        Settings.Default.NextPageToken = "";
        Settings.Default.Save();
        InitializeSettings();
    }

    private void ClearUnpickedMovieButton_Clicked(object sender, RoutedEventArgs e)
    {
        DataService.Instance.ClearUnpickedMovies();
        InitializeSettings();
        
    }

    private void ClearPickedMovieButton_Clicked(object sender, RoutedEventArgs e)
    {
        DataService.Instance.ClearPickedMovies();
        InitializeSettings();
        
    }

    // Toggle the startup setting
    private void StartWithWindowsCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        ToggleStartup(true);
    }

    private void StartWithWindowsCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        ToggleStartup(false);
    }

    private void ToggleStartup(bool enable)
    {
        string appName = "YouTubeMoviePicker2.0";  // Application name or any identifier
        string appPath = System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".exe");
        string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string shortcutPath = System.IO.Path.Combine(startupFolderPath, $"{appName}.lnk");

        if (enable)
        {
            // Create a shortcut to the application in the startup folder
            CreateShortcut(shortcutPath, appPath);
            Settings.Default.StartWithWindows = true;
        }
        else
        {
            // Remove the shortcut from the startup folder
            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
            Settings.Default.StartWithWindows = false;
        }
        Settings.Default.Save();
    }

    private void CreateShortcut(string shortcutPath, string targetPath)
    {
        var shell = new IWshRuntimeLibrary.WshShell();
        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = targetPath;
        shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath);
        shortcut.Save();
    }

    public void LogsButton_Clicked(object sender, RoutedEventArgs e)
    {
        var logFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!System.IO.Directory.Exists(logFolder))
        {
            System.IO.Directory.CreateDirectory(logFolder);
        }

        System.Diagnostics.Process.Start("explorer.exe", logFolder);
    }
}


