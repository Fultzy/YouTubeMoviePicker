using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services;

public static class SlackService
{
   
    /// <summary>
    /// Sends a payload to the Slack WebHook
    /// </summary>
    /// <param name="messageContent"></param>
    /// <param name="title"></param>
    /// <param name="url"></param>
    /// <returns>a string containing "Successful" if sent successful, otherwise "Failure"</returns>
    public static async Task<string> SendPayloadAsync(string messageContent, string title, string url)
    {
        // Create the webHook payload (as JSON)
        var payload = new
        {
            movie_title = title,
            movie_desc = messageContent,
            movie_link = url
        };

        // Serialize the payload to JSON
        var jsonPayload = JsonSerializer.Serialize(payload);

        // Send the message to the Discord WebHook
        using (var client = new HttpClient())
        {
            var logMessage = Settings.Default.EnableVerboseLogs ? jsonPayload : "";
            Logger.SlackLog("==================", false); // Log Separator
            Logger.SlackLog($"Sending Payload to Slack: {logMessage}");

            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(Settings.Default.SlackWebHook, requestContent);

            // Log message Status
            if (response.IsSuccessStatusCode)
            {
                var message = $"Message sent successfully!" + (Settings.Default.EnableVerboseLogs ? response : "");
                Logger.SlackLog(message);
                return "Successful";
            }
            else
            {
                Logger.SlackLog($"Failed to send message. Verify Slack WebHook..\n== Response ==\n{response}");
                return "Failure";
            }
        }
    }
}
