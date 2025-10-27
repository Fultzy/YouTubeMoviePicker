using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;

namespace YouTubeMoviePicker.Services;
public static class OMdbService
{
    private static bool CanPing()
    {
        try
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send("www.OMdbAPI.com");

            Logger.OMdbLog($"========= \nPing Status: {reply.Status}\nRoundtrip Time: {reply.RoundtripTime} ms \nIP Address: {reply.Address} \n=========");

            return true;
        }
        catch (Exception ex)
        {
            Logger.OMdbLog($"Ping Error: {ex.Message}");
            throw new Exception(ex.Message);
        }
    }

    // this method will be used to get movie details from the OMdb API
    public static async Task<Movie> GetMovieDetailsAsync(Movie movie)
    {
        // Build the URL
        var plotLength = Settings.Default.EnableFullPlots ? "full" : "short";
        var client = new HttpClient();
        var uRL = new StringBuilder("http://www.omdbapi.com/?t=");

        uRL.Append(movie.Title.Replace(" ", "+"));
        uRL.Append($"&plot={plotLength}");
        if (string.IsNullOrEmpty(movie.Year)) uRL.Append($"&year={movie.Year}");
        uRL.Append($"&apikey={Settings.Default.OMdbApiKey}");

        // remove api key for logging
        Logger.OMdbLog("=========", false);
        var cleanedUrl = uRL.ToString().Replace(Settings.Default.OMdbApiKey, "API_KEY");
        Logger.OMdbLog($"Fetching {movie.Title} - {movie.Year} {cleanedUrl}...");

        // Fetch movie details from OMdb API
        var response = await client.GetAsync(uRL.ToString());
        var content = await response.Content.ReadAsStringAsync();
        
        // Log the response
        var responseMessage = response.IsSuccessStatusCode ? "Success" : "Failed";
        if (Settings.Default.EnableVerboseLogs) responseMessage = content;
        Logger.OMdbLog($"Response: {responseMessage}");

        // Parse the JSON response into a Movie object
        var omdbMovie = JsonSerializer.Deserialize<Movie>(content);
        
        // merge OMdb details to the movie
        movie.AddOMdbDetails(omdbMovie);

        return movie;
    }
}
