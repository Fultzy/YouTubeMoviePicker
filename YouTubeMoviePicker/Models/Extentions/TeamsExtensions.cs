using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace YouTubeMoviePicker.Models.Extensions
{
    public static class TeamsExtensions
    {
        public static string ToTeamsPayload(this Movie movie)
        {
            // ensure all values are present
            if (movie == null)
            {
                throw new ArgumentNullException(nameof(movie), "Movie object cannot be null");
            }

            if (string.IsNullOrEmpty(movie.Title) ||
                string.IsNullOrEmpty(movie.YTVideoURL))
            {
                throw new ArgumentException("Movie object must have at least Title and YTVideoURL properties set.");
            }

            if (string.IsNullOrEmpty(movie.Poster) && string.IsNullOrEmpty(movie.YTVideoThumbnail))
            {
                throw new ArgumentException("Movie object must have at least Poster or YTVideoThumbnail property set.");
            }

            string jsonPayload = $@"
{{
  ""attachments"": [
    {{
      ""contentType"": ""application/vnd.microsoft.card.adaptive"",
      ""content"": {{
        ""$schema"": ""https://adaptivecards.io/schemas/adaptive-card.json"",
        ""type"": ""AdaptiveCard"",
        ""msteams"": {{  
                ""width"": ""Full""  
            }},  
        ""version"": ""1.5"",
        ""body"": [
          {{
            ""type"": ""TextBlock"",
            ""wrap"": true,
            ""text"": ""Today's Free YouTube Movie is...""
          }},
          {{
            ""type"": ""TextBlock"",
            ""text"": ""{movie.Title ?? ""}"",
            ""wrap"": true,
            ""size"": ""ExtraLarge"",
            ""style"": ""heading"",
            ""weight"": ""Bolder"",
            ""color"": ""Warning"",
            ""isSubtle"": false,
            ""height"": ""stretch"",
            ""horizontalAlignment"": ""Left"",
            ""spacing"": ""None"",
            ""separator"": true
          }},
          {{
            ""type"": ""TextBlock"",
            ""text"": ""{movie.Year ?? ""} | Rated {movie.Rated ?? ""} | {movie.Genre ?? ""}"",
            ""wrap"": true,
            ""spacing"": ""None"",
            ""isSubtle"": true
          }},
          {{
            ""type"": ""TextBlock"",
            ""text"": ""{movie.Plot ?? ""}"",
            ""wrap"": true,
            ""spacing"": ""ExtraSmall"",
            ""separator"": true
          }},
          {{
            ""type"": ""FactSet"",
            ""separator"": ""true"",
            ""facts"": [
              {{ ""title"": ""Cast:"", ""value"": ""{movie.Actors ?? ""}"" }},
              {{ ""title"": ""Directed:"", ""value"": ""{movie.Director ?? ""}"" }},
              {{ ""title"": ""Written:"", ""value"": ""{movie.Writer ?? ""}"" }},
            ]
          }},
          {{
            ""type"": ""FactSet"",
            ""separator"": ""true"",
            ""facts"": [
              {{ ""title"": ""Released:"", ""value"": ""{movie.Released ?? ""}"" }},
              {{ ""title"": ""Box Office:"", ""value"": ""{movie.BoxOffice ?? ""}"" }},
              {{ ""title"": ""Ratings:"", ""value"": ""IMDB {movie.imdbRating ?? ""} ({movie.imdbVotes ?? ""} votes), Metascore {movie.Metascore ?? ""}"" }},
              {{ ""title"": ""Awards:"", ""value"": ""{movie.Awards ?? ""}"" }}
            ]
          }},
          {{
            ""type"": ""Image"",
            ""url"": ""{movie.Poster ?? movie.YTVideoThumbnail ?? ""}"",
          }}
        ],
        ""actions"": [
          {{
            ""type"": ""Action.OpenUrl"",
            ""title"": ""▶️ Watch on YouTube"",
            ""url"": ""{movie.YTVideoURL ?? ""}""
          }}
        ]
      }}
    }}
  ]
}}";

            return jsonPayload;
        }
    }
}
