using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services;
internal class DiscordService
{
    public static async Task<string> SendPayloadAsync(string messageContent)
    {
        // Create the webhook payload (as JSON)
        var payload = new
        {
            content = messageContent
        };

        // Serialize the payload to JSON
        var jsonPayload = JsonSerializer.Serialize(payload);

        // Send the message to the Discord webhook
        using (var client = new HttpClient())
        {
            // TODO: get the movie name for the log somehow. 
            var logMessage = Settings.Default.EnableVerboseLogs ? jsonPayload : "";
            Logger.DiscordLog("==================", false); // Log Separator
            Logger.DiscordLog($"Sending Payload to Discord: {logMessage}");

            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(Settings.Default.DiscordWebHook, requestContent);

            // Log message Status
            if (response.IsSuccessStatusCode)
            {
                Logger.DiscordLog($"Message sent successfully!");
                return "Successful";
            }
            else
            {
                Logger.DiscordLog($"Failed to send message. Verify Discord Webhook..\n== Content ==\n{jsonPayload}\n== Response ==\n{response}");
                return "Failure";
            }
        }
    }
}