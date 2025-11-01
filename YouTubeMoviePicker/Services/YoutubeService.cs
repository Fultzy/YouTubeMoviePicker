using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services;
public static class YouTubeService
{
    private static bool CanPing()
    {
        try
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send("www.YouTube.com");

            Logger.YouTubeLog("=========", false);
            Logger.YouTubeLog($"Ping Status: {reply.Status}");
            Logger.YouTubeLog($"Roundtrip Time: {reply.RoundtripTime} ms");
            Logger.YouTubeLog($"Address: {reply.Address}\n=========");

            return true;
        }
        catch (Exception ex)
        {
            Logger.YouTubeLog($"Ping Error: {ex.Message}");
            throw new Exception(ex.Message);
        }
    }

    public static async Task<List<Movie>> GetMoviesAsync(string query)
    {
        if (!CanPing()) return null;
        var movieList = new List<Movie>();

        try
        {
            var apiKey = Properties.Settings.Default.YouTubeApiKey;
            HttpClient client = new HttpClient();
            string url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&channelId=UCuVPpxrm2VAgpH3Ktln4HXg&q={query.Replace(" ", "+")}+Free+Movie&key={apiKey}";

            var pageCount = 5;
            for (int i = 0; i < pageCount; i++)
            {
                var pageUrl = url;

                if (!string.IsNullOrEmpty(Settings.Default.NextPageToken))
                {
                    pageUrl += $"&pageToken={Settings.Default.NextPageToken}";
                }

                // remove api key for logging
                var cleanedUrl = pageUrl.ToString().Replace(Settings.Default.YouTubeApiKey, "API_KEY");

                Logger.YouTubeLog("=========", false);
                Logger.YouTubeLog($"Fetching {cleanedUrl}...");
                HttpResponseMessage response = await client.GetAsync(pageUrl);

                if (response.IsSuccessStatusCode)
                {
                    Logger.YouTubeLog($"Fetched Movies Success! Page {i + 1} || Token: {Settings.Default.NextPageToken}");

                    movieList.AddRange(await ParsePage(response));
                }
                else
                {
                    Logger.YouTubeLog($"Error: Unable to Fetch Movies: {response.StatusCode}\n{response}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.YouTubeLog($"Error: Unable to Fetch Movies: {ex.Message}\n{ex.StackTrace}");
            throw new Exception(ex.Message + "\n" + ex.StackTrace);
        }

        return movieList;
    }

    private static async Task<List<Movie>> ParsePage(HttpResponseMessage response)
    {
        var movieList = new List<Movie>();
        try
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseBody);
            var root = json.RootElement;
            var items = root.GetProperty("items");

            // apply new page token and handle Last Page
            if (root.TryGetProperty("nextPageToken", out var nextPageToken))
            {
                Settings.Default.NextPageToken = nextPageToken.ToString();
            }
            else
            {
                // clear suggestion and PageToken
                Settings.Default.SuggestionString = string.Empty;
                Settings.Default.NextPageToken = string.Empty;
            }

            // parse each movie
            foreach (var item in items.EnumerateArray())
            {
                var snippet = item.GetProperty("snippet");
                var title = System.Net.WebUtility.HtmlDecode(snippet.GetProperty("title").GetString());
                var videoId = item.GetProperty("id").GetProperty("videoId").GetString();
                var channelId = snippet.GetProperty("channelId").GetString();
                var channelTitle = snippet.GetProperty("channelTitle").GetString();
                var description = System.Net.WebUtility.HtmlDecode(snippet.GetProperty("description").GetString());
                var publishedAt = snippet.GetProperty("publishedAt").GetString();

                // remove all commas from each field This was for some reason. idk some address shit
                videoId = videoId?.Replace(",", "");
                channelId = channelId?.Replace(",", "");
                channelTitle = channelTitle?.Replace(",", "");
                description = description?.Replace(",", "");
                publishedAt = publishedAt?.Replace(",", "");

                // if movie name includes '(1992)' remove it and add it to the year field
                var year = title.Contains("(") && title.Contains(")") ? title.Substring(title.IndexOf("(") + 1, title.IndexOf(")") - title.IndexOf("(") - 1) : string.Empty;

                if (title.Contains("(") && title.Contains(")"))
                {
                    title = title.Replace($"({year})", "").Trim();
                }

                // if movie name includes '[]' remove it and everything in it
                if (title.Contains("[") && title.Contains("]"))
                {
                    title = title.Remove(title.IndexOf("["), title.IndexOf("]") - title.IndexOf("[") + 1).Trim();
                }

                var movie = new Movie
                {
                    Title = title ?? string.Empty,
                    YTVideoId = videoId ?? string.Empty,
                    YTChannelId = channelId ?? string.Empty,
                    YTChannelTitle = channelTitle ?? string.Empty,
                    YTDescription = description ?? string.Empty,
                    YTVideoThumbnail = $"https://img.youtube.com/vi/{videoId}/0.jpg",
                    YTVideoURL = $"https://www.youtube.com/watch?v={videoId}",
                    YTVideoPublishedAt = publishedAt ?? string.Empty
                };

                if (DataService.Instance.UnpickedMoviesList.Any(m => m.YTVideoId == movie.YTVideoId) || DataService.Instance.PickedMoviesList.Any(m => m.YTVideoId == movie.YTVideoId) || movieList.Any(m => m.YTVideoId == movie.YTVideoId))
                {
                    var logMsg = Settings.Default.EnableVerboseLogs ? movie.ToString() : movie.Title;

                    Logger.YouTubeLog($"== Warning == Movie Already Exists: {logMsg}");
                    continue;
                }
                else
                {
                    movieList.Add(movie);
                    var logMsg = Settings.Default.EnableVerboseLogs ? movie.ToString() : movie.Title;

                    Logger.YouTubeLog($"Parsed new Movie: {logMsg}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.YouTubeLog("== ERROR ==", false);
            Logger.YouTubeLog($" Unable to Parse Movie List: {ex.Message}\n Response: \n{ex.StackTrace}");
        }

        return movieList;
    }


}
