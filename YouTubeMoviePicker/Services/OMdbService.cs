using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services;
public static class OMdbService
{
    private static HttpClient Client = new HttpClient();

    public static async Task<Movie> GetMovieDetailsAsync(Movie movie)
    {
        Logger.OMdbLog("==========", false);
        if (string.IsNullOrEmpty(movie?.Title))
        {
            Logger.OMdbLog("Title cannot be null or empty.");
            return movie;
        }

        var plotLength = Settings.Default.EnableFullPlots ? "full" : "short";

        var titleEscaped = Uri.EscapeDataString(movie.Title);
        var uRL = new StringBuilder($"http://www.omdbapi.com/?t={titleEscaped}&plot={plotLength}");

        // Fix: append year only when not empty
        if (!string.IsNullOrEmpty(movie.Year))
            uRL.Append($"&year={Uri.EscapeDataString(movie.Year)}");

        if (string.IsNullOrEmpty(Settings.Default.OMdbApiKey))
        {
            Logger.OMdbLog("OMdb API key is not configured.");
            return movie;
        }
        
        uRL.Append($"&apikey={Settings.Default.OMdbApiKey}");

        var cleanedUrl = uRL.ToString().Replace(Settings.Default.OMdbApiKey, "API_KEY");
        Logger.OMdbLog($"Fetching {movie.Title} - {movie.Year} {cleanedUrl}...");

        var response = await Client.GetAsync(uRL.ToString());
        var content = await response.Content.ReadAsStringAsync();

        Logger.OMdbLog($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        if (Settings.Default.EnableVerboseLogs) Logger.OMdbLog($"Response body: {content}");

        if (!response.IsSuccessStatusCode)
        {
            Logger.OMdbLog("OMDb request failed.");
            return movie;
        }

        // sanity check
        var trimmed = content?.TrimStart();
        if (string.IsNullOrEmpty(trimmed) || (trimmed[0] != '{' && trimmed[0] != '['))
        {
            Logger.OMdbLog("Response is not JSON. First chars: " + (trimmed?.Substring(0, Math.Min(200, trimmed.Length)) ?? "<empty>"));
            return movie;
        }

        try
        {
            var omdbObject = JsonSerializer.Deserialize<JsonObject>(content);
            movie.AddOMdbDetails(omdbObject);
        }
        catch (JsonException ex)
        {
            Logger.OMdbLog("JSON parse error: " + ex.Message);
            if (Settings.Default.EnableVerboseLogs) Logger.OMdbLog($"Raw response: {content}");
        }

        return movie;
    }
}
