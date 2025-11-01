using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace YouTubeMoviePicker.Utility
{
    public static class FromJson
    {
        public static string Object(JsonObject obj, string key, out string value)
        {
            value = string.Empty;
            if (obj == null || key == null) return value;

            value = obj[key]?.ToString() ?? string.Empty;

            if (value == "N/A") value = string.Empty;
            return value;
        }
    }
}
