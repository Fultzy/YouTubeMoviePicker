using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Models.Extensions;
using YouTubeMoviePicker.Properties;

namespace YouTubeMoviePicker.Utility
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

            // increase score by averaged normalized rating
            if (movie.Ratings != null)
            {
                score += movie.GetRatingNormalized();
            }

            return score;
        }

        public static List<Movie> SortMoviesByAverageRating(List<Movie> movies)
        {
            // Normalize and calculate the average rating
            var scoredMovies = movies.Select(m => new
            {
                Movie = m,
                NormalizedAverageRating = m.GetRatingNormalized()
            })

            .OrderByDescending(x => x.NormalizedAverageRating) // Sort by average rating (best first)
            .Select(x => x.Movie)
            .ToList();

            Settings.Default.SortBy = "Best Rating";
            return scoredMovies;
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
