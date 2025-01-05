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
    https://discordapp.com/api/webhooks/1315158353470558269/N5HM_1CP3ED9uq8tA3Ff0edCTH3wauMeTtUT1MROgwHU4DFKN3eVnZ0XDm8dp2tGo93A

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
        
}
