using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeMoviePicker.Models.Extensions
{
    internal static class SlackExtensions
    {
        public static string ToSlackPayload(this Movie movie)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(movie.Year))
                sb.Append($"{movie.Year}");

            if (!string.IsNullOrEmpty(movie.Rated))
                sb.Append($" - Rated {movie.Rated}");

            if (!string.IsNullOrEmpty(movie.Genre))
                sb.Append($" - {movie.Genre}");

            if (!string.IsNullOrEmpty(movie.Plot))
            {
                sb.Append($"\n\n     {movie.Plot}\n");
            }
            else // if no plot from OMdb, use YTDescription
            {
                sb.Append($"\n\n     {movie.YTDescription}");
            }

            if (!string.IsNullOrEmpty(movie.Actors) && movie.Actors != "N/A")
                sb.Append($"\n:person_in_tuxedo::skin-tone-4:  Cast:  {movie.Actors}");

            if (!string.IsNullOrEmpty(movie.Director) && movie.Director != "N/A")
                sb.Append($"\n:film_projector: Directed by: {movie.Director}");

            if (!string.IsNullOrEmpty(movie.Writer) && movie.Writer != "N/A")
                sb.Append($"\n:writing_hand: Written by:  {movie.Writer}");

            if (!string.IsNullOrEmpty(movie.Released) || !string.IsNullOrEmpty(movie.Production) || !string.IsNullOrEmpty(movie.BoxOffice) || !string.IsNullOrEmpty(movie.Awards))
            {
                sb.Append("\n"); // Separator if needed
            }

            if (!string.IsNullOrEmpty(movie.Released) && movie.Released != "N/A")
                sb.Append($"\nReleased:  {movie.Released}");

            if (!string.IsNullOrEmpty(movie.Production) && movie.Production != "N/A")
                sb.Append($"\nProduction:  {movie.Production}");

            if (!string.IsNullOrEmpty(movie.BoxOffice) && movie.BoxOffice != "N/A")
                sb.Append($"\nBox Office:  {movie.BoxOffice}");

            if (!string.IsNullOrEmpty(movie.Awards) && movie.Awards != "N/A")
                sb.Append($"\nAwards:  {movie.Awards}");

            // determine ratings
            if (!string.IsNullOrEmpty(movie.Metascore) || !string.IsNullOrEmpty(movie.imdbRating) || !string.IsNullOrEmpty(movie.imdbVotes))
            {
                sb.Append("\n");
            }

            if (!string.IsNullOrEmpty(movie.Metascore) && movie.Metascore != "N/A")
                sb.Append($"Metascore: {movie.Metascore}/100     ");

            if (!string.IsNullOrEmpty(movie.imdbRating) && movie.imdbRating != "N/A")
                sb.Append($"IMdb Rating: {movie.imdbRating}/10 ");

            if (!string.IsNullOrEmpty(movie.imdbVotes) && movie.imdbVotes != "N/A")
                sb.Append($"- Votes {movie.imdbVotes}");


            sb.Append("\n"); // Separator 

            // determine disclaimer
            var disclaimer = "\nEnjoy the movie! :popcorn:";

            if (string.IsNullOrEmpty(movie.Plot) && !string.IsNullOrEmpty(movie.YTDescription))
                disclaimer = "\nEnjoy the movie! :popcorn: (no additional details available..)";

            if (!string.IsNullOrEmpty(movie.Plot))
                disclaimer = "\nMovie details may sometimes be inaccurate.. :shrug:";

            sb.Append(disclaimer);

            return sb.ToString();
        }
    }
}
