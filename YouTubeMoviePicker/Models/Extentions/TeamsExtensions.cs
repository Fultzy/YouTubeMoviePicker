using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media;

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

            if (string.IsNullOrEmpty(movie.Title))
            {
                throw new ArgumentException("Movie object must have Title properties set.");
            }

            var payload = CreatePayload(movie);

            var options = new JsonSerializerOptions { WriteIndented = true };
            return payload.ToJsonString(options);

        }

        private static JsonObject CreatePayload(Movie movie)
        {
            var rating = movie.GetRatingNormalized();
            return new JsonObject
            {
                ["attachments"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["contentType"] = "application/vnd.microsoft.card.adaptive",
                        ["content"] = new JsonObject
                        {
                            ["$schema"] = "https://adaptivecards.io/schemas/adaptive-card.json",
                            ["type"] = "AdaptiveCard",
                            ["msteams"] = new JsonObject { ["width"] = "Full" },
                            ["version"] = "1.5",
                            ["body"] = new JsonArray
                            {
                                new JsonObject
                                {
                                    ["type"] = "TextBlock",
                                    ["wrap"] = true,
                                    ["text"] = "Today's Free YouTube Movie is..."
                                },
                               
                                new JsonObject
                                {
                                    ["type"] = "ColumnSet",
                                    ["spacing"] = "large",
                                    ["columns"] = new JsonArray
                                    {
                                        new JsonObject
                                        {
                                            ["type"] = "Column",
                                            ["width"] = "stretch",
                                            ["items"] =  new JsonArray
                                            {
                                                 new JsonObject
                                                {
                                                    ["type"] = "TextBlock",
                                                    ["text"] = movie.Title ?? string.Empty,
                                                    ["wrap"] = true,
                                                    ["size"] = "ExtraLarge",
                                                    ["style"] = "heading",
                                                    ["weight"] = "Bolder",
                                                    ["color"] = "Warning",
                                                    ["isSubtle"] = false,
                                                    ["height"] = "stretch",
                                                    ["horizontalAlignment"] = "Left",
                                                    ["spacing"] = "None",
                                                    ["separator"] = true
                                                },
                                                new JsonObject
                                                {
                                                    ["type"] = "TextBlock",
                                                    ["text"] = movie.GetSubtitleString(),
                                                    ["wrap"] = true,
                                                    ["spacing"] = "None",
                                                    ["isSubtle"] = true
                                                },
                                            }
                                        },
                                        new JsonObject
                                        {
                                            ["type"] = "Column",
                                            ["width"] = "auto",
                                            ["items"] =  new JsonArray
                                            {
                                                new JsonObject
                                                {
                                                    ["type"] = "TextBlock",
                                                    ["text"] = rating + "/10",
                                                    ["size"] = "extraLarge",
                                                    ["weight"] = "bolder",
                                                    ["color"] = movie.GetFultzyMeterTeamsColor(rating),
                                                    ["fontType"] = "Monospace",
                                                    ["horizontalAlignment"] = "center",
                                                },
                                                new JsonObject
                                                {
                                                    ["type"] = "TextBlock",
                                                    ["text"] = "Fultzy Meter",
                                                    ["size"] = "Small",
                                                    ["weight"] = "bolder",
                                                    ["spacing"] = "none",
                                                    ["color"] = movie.GetFultzyMeterTeamsColor(rating),
                                                    ["fontType"] = "Monospace",
                                                    ["horizontalAlignment"] = "center",
                                                },
                                            }
                                        },
                                    }
                                },

                                new JsonObject
                                {
                                    ["type"] = "TextBlock",
                                    ["text"] = movie.GetMoviePlotString(),
                                    ["wrap"] = true,
                                    ["spacing"] = "medium",
                                    ["separator"] = true
                                },
                                new JsonObject
                                {
                                    ["type"] = "ColumnSet",
                                    ["spacing"] = "large",
                                    ["columns"] = new JsonArray
                                    {
                                        new JsonObject
                                        {
                                            ["type"] = "Column",
                                            ["width"] = "auto",
                                            ["items"] =  new JsonArray
                                            {
                                                 new JsonObject
                                                 {
                                                    ["type"] = "Image",
                                                    ["url"] = movie.Poster ?? movie.YTVideoThumbnail ?? string.Empty
                                                 }
                                            }
                                        },
                                        new JsonObject
                                        {
                                            ["type"] = "Column",
                                            ["width"] = "auto",
                                            ["items"] =  new JsonArray
                                            {
                                                new JsonObject
                                                {
                                                    ["type"] = "FactSet",
                                                    ["facts"] = CreateCreditsArray(movie)
                                                },
                                                new JsonObject
                                                {
                                                    ["type"] = "FactSet",
                                                    ["separator"] = true,
                                                    ["facts"] = CreateFactsArray(movie)
                                                },
                                            }
                                        },
                                    }
                                },

                            },
                            ["actions"] = new JsonArray
                            {
                                new JsonObject
                                {
                                    ["type"] = "Action.OpenUrl",
                                    ["title"] = "▶️ Watch on YouTube",
                                    ["url"] = movie.YTVideoURL ?? string.Empty
                                }
                            }
                        }
                    }
                }
            };
        }

        private static JsonArray CreateFactsArray(Movie movie)
        {
            var factsArray = new JsonArray{};

            if (movie.Released != null && movie.Released != "N/A")
            {
                factsArray.Add(new JsonObject { ["title"] = "Released:", ["value"] = movie.Released ?? string.Empty });
            }

            if (movie.BoxOffice != null && movie.BoxOffice != "N/A")
            {
                factsArray.Add(new JsonObject { ["title"] = "Box Office:", ["value"] = movie.BoxOffice ?? string.Empty });
            }

            if (movie.Ratings != null)
            {
                foreach (var rating in movie.Ratings)
                {
                    
                    factsArray.Add(new JsonObject { ["title"] = rating.Source, ["value"] = rating.Value });
                }
            }
            
            return factsArray;
        }

        private static JsonArray CreateCreditsArray(Movie movie)
        {
            var factsArray = new JsonArray{};

            if (movie.Actors != null && movie.Actors != "N/A")
            {
                factsArray.Add(new JsonObject { ["title"] = "Cast:", ["value"] = movie.Actors ?? string.Empty });
            }

            if (movie.Director != null && movie.Director != "N/A")
            {
                factsArray.Add(new JsonObject { ["title"] = "Directed:", ["value"] = movie.Director ?? string.Empty });
            }

            if (movie.Writer != null && movie.Writer != "N/A")
            {
                factsArray.Add(new JsonObject { ["title"] = "Written:", ["value"] = movie.Writer ?? string.Empty });
            }

            return factsArray;
        }


        //private static JsonObject JsonChoice2(Movie movie)
        //{
        //    return new JsonObject
        //    {
        //        ["attachments"] = new JsonArray
        //        {
        //            new JsonObject
        //            {
        //                ["contentType"] = "application/vnd.microsoft.card.adaptive",
        //                ["content"] = new JsonObject
        //                {
        //                    ["$schema"] = "https://adaptivecards.io/schemas/adaptive-card.json",
        //                    ["type"] = "AdaptiveCard",
        //                    ["msteams"] = new JsonObject { ["width"] = "Full" },
        //                    ["version"] = "1.5",
        //                    ["body"] = new JsonArray
        //                    {
        //                        new JsonObject
        //                        {
        //                            ["type"] = "TextBlock",
        //                            ["wrap"] = true,
        //                            ["text"] = "Today's Free YouTube Movie is..."
        //                        },
        //                        new JsonObject
        //                        {
        //                            ["type"] = "TextBlock",
        //                            ["text"] = movie.Title ?? string.Empty,
        //                            ["wrap"] = true,
        //                            ["size"] = "ExtraLarge",
        //                            ["style"] = "heading",
        //                            ["weight"] = "Bolder",
        //                            ["color"] = "Warning",
        //                            ["isSubtle"] = false,
        //                            ["height"] = "stretch",
        //                            ["horizontalAlignment"] = "Left",
        //                            ["spacing"] = "None",
        //                            ["separator"] = true
        //                        },


        //                        new JsonObject
        //                        {
        //                            ["type"] = "ColumnSet",
        //                            ["separator"] = true,
        //                            ["columns"] = new JsonArray
        //                            {
        //                                new JsonObject
        //                                {
        //                                    ["type"] = "Column",
        //                                    //["width"] = "auto",
        //                                    ["items"] =  new JsonArray
        //                                    {
        //                                        new JsonObject
        //                                        {
        //                                            ["type"] = "FactSet",
        //                                            ["facts"] = new JsonArray
        //                                            {
        //                                                new JsonObject { ["title"] = "Cast:", ["value"] = movie.Actors ?? string.Empty },
        //                                                new JsonObject { ["title"] = "Directed:", ["value"] = movie.Director ?? string.Empty },
        //                                                new JsonObject { ["title"] = "Written:", ["value"] = movie.Writer ?? string.Empty }
        //                                            }
        //                                        },
        //                                    }
        //                                },
        //                                new JsonObject
        //                                {
        //                                    ["type"] = "Column",
        //                                    //["width"] = "stretch",
        //                                    ["items"] =  new JsonArray
        //                                    {
        //                                        new JsonObject
        //                                        {
        //                                            ["type"] = "FactSet",
        //                                            ["separator"] = true,
        //                                            ["facts"] = new JsonArray
        //                                            {
        //                                                new JsonObject { ["title"] = "Released:", ["value"] = movie.Released ?? string.Empty },
        //                                                new JsonObject { ["title"] = "Box Office:", ["value"] = movie.BoxOffice ?? string.Empty },
        //                                                new JsonObject { ["title"] = "Ratings:", ["value"] = $"{movie.GetRatingsString()}" },
        //                                                new JsonObject { ["title"] = "Awards:", ["value"] = movie.Awards ?? string.Empty }
        //                                            }
        //                                        },
        //                                    }
        //                                },
        //                            }
        //                        },



        //                        new JsonObject
        //                        {
        //                            ["type"] = "ColumnSet",
        //                            ["separator"] = true,
        //                            ["columns"] = new JsonArray
        //                            {
        //                                new JsonObject
        //                                {
        //                                    ["type"] = "Column",
        //                                    ["width"] = "auto",
        //                                    ["items"] =  new JsonArray
        //                                    {
        //                                         new JsonObject
        //                                         {
        //                                            ["type"] = "Image",
        //                                            ["url"] = movie.Poster ?? movie.YTVideoThumbnail ?? string.Empty
        //                                         }
        //                                    }
        //                                },
        //                                new JsonObject
        //                                {
        //                                    ["type"] = "Column",
        //                                    ["separator"] = true,
        //                                    ["width"] = "stretch",
        //                                    ["items"] =  new JsonArray
        //                                    {
        //                                        new JsonObject
        //                                        {
        //                                            ["type"] = "TextBlock",
        //                                            ["text"] = movie.Plot ?? string.Empty,
        //                                            ["wrap"] = true,
        //                                            ["spacing"] = "ExtraSmall",
        //                                            ["separator"] = true
        //                                        },
        //                                    }
        //                                },
        //                            }
        //                        },
        //                    },
        //                    ["actions"] = new JsonArray
        //                    {
        //                        new JsonObject
        //                        {
        //                            ["type"] = "Action.OpenUrl",
        //                            ["title"] = "▶️ Watch on YouTube",
        //                            ["url"] = movie.YTVideoURL ?? string.Empty
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    };

        //}
    }

}
