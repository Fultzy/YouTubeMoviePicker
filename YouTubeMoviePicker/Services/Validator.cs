using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeMoviePicker.Services;

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
            result = "Slack WebHook Cannot Be Blank,\nOtherwise Disable it";
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

        // to the loser that found my testing webhook, you're welcome. hope you had fun being racist to literally no one.

        if (webHook.Contains(' '))
        {
            result = "Discord WebHook Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(webHook))
        {
            result = "Discord WebHook Cannot Be Blank,\nOtherwise Disable it";
        }

        return result;
    }

    internal static string IsValidTeamsWebHook(string apiKey)
    {
        var result = "pass";

        if (apiKey.Contains(' '))
        {
            result = "Teams Api Key Cannot Contain Spaces";
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            result = "Teams Api Key Cannot Be Blank,\nOtherwise Disable it";
        }

        return result;
    }
}
