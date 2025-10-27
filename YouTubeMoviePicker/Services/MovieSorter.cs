using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;

namespace YouTubeMoviePicker.Services
{
    public class MovieSorter
    {
        public static List<Movie> SortMoviesByRelevance(List<Movie> movies, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return movies; // No search term, return the original list
            }

            // Normalize search term to lowercase for case-insensitive comparison
            string searchLower = searchTerm.ToLower();

            // Split search term into individual words
            var searchWords = searchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Create a list of movies with relevance score
            var scoredMovies = movies.Select(m => new
            {
                Movie = m,
                Score = GetRelevanceScore(m, searchLower) + searchWords.Sum(word => GetRelevanceScore(m, word))
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Movie)
            .ToList();

            Settings.Default.SortBy = "Relevance";
            return scoredMovies;
        }

        private static int GetRelevanceScore(Movie movie, string searchTerm)
        {
            int score = 0;

            // Check title
            if (movie.Title != null && movie.Title.ToLower().Contains(searchTerm))
            {
                score += 4;  // Title matches, higher score
            }

            // Check genre
            if (movie.Genre != null && movie.Genre.ToLower().Contains(searchTerm))
            {
                score += 2;  // Genre matches, medium score
            }

            // Check actors, directors, writers (these are now strings, not lists)
            if (movie.Actors != null && movie.Actors.ToLower().Contains(searchTerm))
            {
                score += 1;  // Actors match, lower score
            }

            if (movie.Director != null && movie.Director.ToLower().Contains(searchTerm))
            {
                score += 1;  // Directors match, lower score
            }

            if (movie.Writer != null && movie.Writer.ToLower().Contains(searchTerm))
            {
                score += 1;  // Writers match, lower score
            }

            // Check plot
            if (movie.Plot != null && movie.Plot.ToLower().Contains(searchTerm))
            {
                score += 1;  // Plot matches, lower score
            }

            // TODO: why is this so complicated?
            // increase score if matching and is highly rated
            if (((movie.imdbRating != null && movie.imdbRating != "N/A") && Convert.ToDouble(movie.imdbRating) > 4.5) || ((movie.Metascore != null && movie.Metascore != "N/A") && Convert.ToInt32(movie.Metascore) > 40))
            {
                score += 2; // highly rated, medium-high score
            }

            return score;
        }


        public static List<Movie> SortMoviesByAverageRating(List<Movie> movies)
        {
            // Normalize and calculate the average rating
            var scoredMovies = movies.Select(m => new
            {
                Movie = m,
                NormalizedAverageRating = GetNormalizedAverageRating(m)
            })

            .OrderByDescending(x => x.NormalizedAverageRating) // Sort by average rating (best first)
            .Select(x => x.Movie)
            .ToList();

            Settings.Default.SortBy = "Best Rating";
            return scoredMovies;
        }

        private static double GetNormalizedAverageRating(Movie movie)
        {
            // Normalize the IMDB Rating to a 1-10 scale (if valid)
            double normalizedImdb = GetNormalizedImdbRating(movie.imdbRating);

            // Normalize Metascore from 1-100 to a 1-10 scale (if valid)
            double normalizedMetascore = GetNormalizedMetascore(movie.Metascore);

            // Handle the case where one of the ratings is missing
            if (normalizedImdb == 0 && normalizedMetascore == 0)
            {
                return 0; // Both ratings are invalid, return 0
            }

            // If one is valid, use the valid rating only
            if (normalizedImdb == 0)
            {
                return normalizedMetascore; // Only metascore is valid
            }

            if (normalizedMetascore == 0)
            {
                return normalizedImdb; // Only imdb rating is valid
            }

            // If both ratings are valid, calculate the average
            return (normalizedImdb + normalizedMetascore) / 2;
        }

        private static double GetNormalizedImdbRating(string imdbRating)
        {
            // If the IMDB Rating is valid (not null or "N/A"), parse it to a double
            if (!string.IsNullOrEmpty(imdbRating) && imdbRating != "N/A" && double.TryParse(imdbRating, out double parsedImdb))
            {
                return parsedImdb; // Return as is since it's already in the 1-10 range
            }
            return 0; // Treat invalid or missing IMDB Rating as 0
        }

        private static double GetNormalizedMetascore(string metascore)
        {
            // If the Metascore is valid (not null or "N/A"), parse it to an integer
            if (!string.IsNullOrEmpty(metascore) && metascore != "N/A" && int.TryParse(metascore, out int parsedMetascore))
            {
                return parsedMetascore / 10.0; // Normalize to 1-10 range
            }
            return 0; // Treat invalid or missing Metascore as 0
        }

        internal static List<Movie> RandomizeMovies(List<Movie> unpickedMoviesList)
        {
            // Randomize the list of unpicked movies
            var random = new Random();
            var randomizedMovies = unpickedMoviesList.OrderBy(x => random.Next()).ToList();

            Settings.Default.SortBy = "Random";
            return randomizedMovies;
        }
    }
}
