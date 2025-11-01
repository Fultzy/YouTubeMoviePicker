using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services
{
    public static class TeamsService
    {
        /// <summary>
        /// Sends a payload to the Teams WebHook
        /// </summary>
        /// <param name="messageContent"></param>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns>a string containing "Successful" if sent successful, otherwise "Failure"</returns>
        public static async Task<string> SendPayloadAsync(string jsonPayload)
        {
            var webhookUrl = Settings.Default.TeamsWebHook;

            //var jsonPayload = JsonSerializer.Serialize(messageContent);

            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");


            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(webhookUrl, content);

            // Log message Status
            if (response.IsSuccessStatusCode)
            {
                var message = $"Message sent successfully!" + (Settings.Default.EnableVerboseLogs ? response : "");
                Logger.TeamsLog(message);
                return "Successful";
            }
            else
            {
                Logger.TeamsLog($"Failed to send message. Verify Slack WebHook..\n== Response ==\n{response}");
                return "Failure";
            }
        }


        // This works!!
        public static async Task SendPayloadAsync1()
        {
            string webhookUrl = Settings.Default.TeamsWebHook;

            string jsonPayload = @"
            {
              ""type"": ""AdaptiveCard"",
              ""attachments"": [
                {
                  ""contentType"": ""application/vnd.microsoft.card.adaptive"",
                  ""content"": {
                    ""$schema"": ""https://adaptivecards.io/schemas/adaptive-card.json"",
                    ""type"": ""AdaptiveCard"",
                    ""version"": ""1.5"",
                    ""body"": [
                      {
                        ""type"": ""TextBlock"",
                        ""text"": ""Hello from C#"",
                        ""wrap"": true
                      }
                    ]
                  }
                }
              ]
            }";

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(webhookUrl, content);

                string result = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine($"Response: {response.StatusCode} - {result}");

                
            }
        }
    }
}

