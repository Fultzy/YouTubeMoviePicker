using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Utility;

namespace YouTubeMoviePicker.Services
{
    internal static class PosterService
    {
        // Reuse a single HttpClient instance
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static async Task<BitmapImage> Fetch(Movie movie)
        {
            if (movie == null) throw new ArgumentNullException(nameof(movie));

            // Helper to try load from a URL (returns null on failure)
            static async Task<BitmapImage?> LoadFromUrlAsync(Uri uri)
            {
                try
                {
                    var bytes = await _httpClient.GetByteArrayAsync(uri).ConfigureAwait(false);
                    using var ms = new MemoryStream(bytes);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // ensures data is loaded immediately
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze(); // safe to use across threads / UI
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }

            // Prefer OMDb poster if provided and not "N/A"
            if (!string.IsNullOrWhiteSpace(movie.Poster) && !string.Equals(movie.Poster, "N/A", StringComparison.OrdinalIgnoreCase))
            {
                if (Uri.TryCreate(movie.Poster, UriKind.Absolute, out var posterUri))
                {
                    var img = await LoadFromUrlAsync(posterUri).ConfigureAwait(false);
                    if (img != null)
                    {
                        return img;
                    }

                    Logger.ExceptionLog($"Poster image fetch failed (OMDb): {movie.YTVideoId} - {movie.Title}");
                }
            }

            // Fallback to YouTube thumbnail
            if (!string.IsNullOrWhiteSpace(movie.YTVideoThumbnail) && Uri.TryCreate(movie.YTVideoThumbnail, UriKind.Absolute, out var thumbUri))
            {
                var img = await LoadFromUrlAsync(thumbUri).ConfigureAwait(false);
                if (img != null)
                {
                    return img;
                }
            }

            var msg = $"Failed to fetch poster image for {movie.YTVideoId} - {movie.Title}. Tried: {movie.Poster} and {movie.YTVideoThumbnail}";
            Logger.ExceptionLog(msg);
            throw new InvalidOperationException(msg);
        }
    }
}