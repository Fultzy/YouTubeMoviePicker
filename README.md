# YouTube Movie Picker

## Features

- Automaticly post free daily movies found on YouTube to your Discord or Slack via a simple WebHook!
- Choose when API fetches and daily movie posting occures
- Uses OMdb to get additional details about each movie.
- Formatted posting messages for Discord and Slack.
- Automated data managment, will periodicly clear movies to ensure working URLs since Youtube will occasionally take movies down. 

## Requirements

- .NET 6.0 - (will prompt to install on first run)
- an OMdb API Key
- a YouTube API Key
- a Discord WebHook URL (optional)
- a Slack WebHook URL (optional)

## installation 

- Download the Zip file by clicking [Here](https://github.com/Fultzy/YouTubeMoviePicker/releases/download/Initial/YouTube.MoviePicker2.zip) or by checking [Releases](https://github.com/Fultzy/YouTubeMoviePicker/releases) 
- Extract the Zip file
- Run YoutubeMoviePicker.exe
- Follow the prompt to install .Net if it requires


## Usage

1. Launch the application.
2. Configure your settings in the settings menu.
3. Select the days, time and check off make it so to enable daily movie posting
4. Use the "Fetch YouTube" button to the right of the suggestion box to fetch movies based on your suggestions.
5. Select a movie from the list to view its details
     - Double click a movie to move it to the front of the list
     - Right click for additional operations (ie: remove Or send to rear). 
6. Use the "Post Now" button to post the selected movie to Discord and/or Slack.
7. The application will automatically schedule tasks to clear unpicked movies to maintain fresh, working, youtube movie links since movies are often removed after some time.

## Configuration

The application settings can be configured through the settings menu in the application. Key settings include:

- `EnableDebug`: Enable or disable debug mode.
- `EnablePostButton`: Show or hide the Post Now button.
- `EnableAutonomousDeployment`: Enable or disable autonomous deployment (automatic movie posting).
- `DiscordWebHook`: URL for the Discord webhook.
- `SlackWebHook`: URL for the Slack webhook.
- `OMdbApiKey`: API key for the OMdb API.
- `YouTubeApiKey`: API key for the YouTube API.

## Running From Visual Studio

1. Clone the repository: git clone https://github.com/yourusername/YouTubeMoviePicker.git
2. Open the solution in Visual Studio 2022.
3. Restore the NuGet packages: dotnet restore
4. Build and run the application.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [OMdb API](http://www.omdbapi.com/)
- [YouTube Data API](https://developers.google.com/youtube/v3)
- [Discord Webhooks](https://discord.com/developers/docs/resources/webhook)
- [Slack Webhooks](https://api.slack.com/messaging/webhooks)
