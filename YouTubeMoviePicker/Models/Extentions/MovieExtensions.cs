using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace YouTubeMoviePicker.Models.Extensions
{
    public static class MovieExtensions
    {
        public static string GetSubtitleString(this Movie movie)
        {
            if (string.IsNullOrEmpty(movie.Released))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(movie.Year)) sb.Append($"{movie.Year} ");
            if (!string.IsNullOrEmpty(movie.Rated)) sb.Append($"| {movie.Rated} ");
            if (!string.IsNullOrEmpty(movie.Runtime)) sb.Append($"| {movie.Runtime} ");
            if (!string.IsNullOrEmpty(movie.Genre)) sb.Append($"| {movie.Genre}");

            return sb.ToString();
        }

        public static string GetMoviePlotString(this Movie movie)
        {
            if (string.IsNullOrEmpty(movie.Plot) || movie.Plot == "N/A")
            {
                if (string.IsNullOrEmpty(movie.YTDescription))
                {
                    return string.Empty;
                }
                movie.Plot = movie.YTDescription;
            }

            movie.Plot = movie.Plot.Replace('"', '`');

            const int maxLength = 500;
            if (movie.Plot.Length <= maxLength)
            {
                return movie.Plot;
            }
            else
            {
                return movie.Plot.Substring(0, maxLength) + "...";
            }
        }

        public static string GetRatingsString(this Movie movie)
        {
            var ratingsList = new List<string>();
            if (movie.Ratings != null)
            {
                foreach (var rating in movie.Ratings)
                {
                    ratingsList.Add($"{rating.Source}: {rating.Value}");
                }
            }
            return string.Join(" | ", ratingsList);
        }

        // Normalize different rating sources to a common scale
        // will include IMDB, Metascore, and Rotten Tomatoes.
        public static int GetRatingNormalized(this Movie movie)
        {
            int score = 0;
            if (movie.Ratings != null)
            {
                if (movie.Ratings.Count == 0) return 0;
                foreach (var rating in movie.Ratings)
                {
                    if (rating.Source == "Internet Movie Database" || rating.Source == "IMDb")
                    {
                        // IMDB rating is out of 10
                        if (double.TryParse(rating.Value.Split('/')[0], out double imdbRating))
                        {
                            score += (int)(imdbRating); // Return as is
                        }
                    }
                    else if (rating.Source == "Metacritic")
                    {
                        // Metascore is out of 100
                        if (int.TryParse(rating.Value.Split('/')[0], out int metascore))
                        {
                            score += metascore / 10; // Normalize to 1-10 scale
                        }
                    }
                    else if (rating.Source == "Rotten Tomatoes")
                    {
                        // Rotten Tomatoes is a percentage
                        if (rating.Value.EndsWith("%") && int.TryParse(rating.Value.TrimEnd('%'), out int rtScore))
                        {
                            score += rtScore / 10; // Normalize to 1-10 scale
                        }
                    }
                }
                score = score / movie.Ratings.Count; // Average score
            }

            return score;
        }

        public static Brush GetFultzyMeterColor(this Movie movie, int rating)
        {
            if (rating > 0)
            {
                if (rating >= 7)
                {
                    return Brushes.GreenYellow;
                }
                else if (rating >= 4)
                {
                    return Brushes.Orange;
                }
                else
                {
                    return Brushes.Red;
                }
            }
            return Brushes.Gray; // Default color for invalid or missing ratings
        }

        public static string GetFultzyMeterTeamsColor(this Movie movie, int rating)
        {
            if (rating > 0)
            {
                if (rating >= 7)
                {
                    return "good";
                }
                else if (rating >= 4)
                {
                    return "warning";
                }
                else
                {
                    return "attention";
                }
            }
            return "default"; // Default color for invalid or missing ratings
        }
    }
}