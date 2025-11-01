using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeMoviePicker.Utility;

public static class Validator
{
    public static string IsValidYouTubeApiKey(string apiKey)
    {
        var result = "pass";
        
        // TODO: Add more validation here
        
        if (apiKey.Contains(' '))
        {
            result = "YouTube Api Key Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            result = "YouTube Api Key Cannot Be Blank";
        }


        return result;
    }

    public static string IsValidSlackWebHook(string webHook)
    {
        var result = "pass";

        // TODO: Add more validation here
        
        if (!webHook.Contains("https://hooks.slack.com/"))
        {
            result = "Slack WebHook Must Contain 'https://hooks.slack.com/'";
        }

        if (webHook.Contains(' '))
        {
            result = "Slack WebHook Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(webHook))
        {
            result = "Slack WebHook Cannot Be Blank\nOtherwise Disable it";
        }

        return result;
    }

    public static string IsValidOMdbApiKey(string apiKey)
    {
        var result = "pass";

        // TODO: Add more validation here

        if (apiKey.Contains(' '))
        {
            result = "OMdb Api Key Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            result = "OMdb Api Key Cannot Be Blank";
        }

        return result;
    }

    public static string IsValidDiscordWebHook(string webHook)
    {
        var result = "pass";

        // TODO: Add more validation here

        if (!webHook.Contains("https://discord.com/api/webhooks/") && !webHook.Contains("https://discordapp.com/api/webhooks/"))
        {
            result = "Discord WebHook Must Contain 'https://discord.com/api/webhooks/' or 'https://discordapp.com/api/webhooks/'";
        }

        if (webHook.Contains(' '))
        {
            result = "Discord WebHook Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(webHook))
        {
            result = "Discord WebHook Cannot Be Blank,\nOtherwise Disable it";
        }

        if (!webHook.StartsWith("https://"))
        {
            result = "Discord Webhook Must Start With 'https://'";
        }

        return result;
    }

    internal static string IsValidTeamsWebHook(string webHook)
    {
        var result = "pass";

        if (webHook.Contains(' '))
        {
            result = "Teams Webhook Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(webHook))
        {
            result = "Teams Webhook Cannot Be Blank,\nOtherwise Disable it";
        }

        if (!webHook.StartsWith("https://"))
        {
            result = "Teams Webhook Must Start With 'https://'";
        }

        return result;
    }
}
