using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Properties;
using YouTubeMoviePicker.Utility;
using static System.Windows.Forms.Design.AxImporter;

namespace YouTubeMoviePicker.Services;

public class DataService
{
    public List<Movie> UnpickedMoviesList = new List<Movie>();
    public List<Movie> PickedMoviesList = new List<Movie>();
    //public List<Poster> MoviePosters = new List<Poster>();

    public EventHandler<DataChangeArgs> DataChanged;

    // A Singleton
    private static DataService _instance;
    public static DataService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataService();
            }
            return _instance;
        }
    }

    // General
    public void LoadData()
    {
        // Load UnpickedMovies from a json file
        var unpickedPath = Path.Combine(Environment.CurrentDirectory,"Data", "UnpickedMovies.json");
        if (File.Exists(unpickedPath))
        {
            var unpickedJson = File.ReadAllText(unpickedPath);
            if (unpickedJson.Length > 0)
            {
                UnpickedMoviesList = JsonSerializer.Deserialize<List<Movie>>(unpickedJson);
            }
        }
        else
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "Data");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Logger.MainLog("Creating New UnpickedMovies.json");
            File.Create(unpickedPath).Dispose();
        }

        // Load PickedMovies from a json file
        var pickedPath = Path.Combine(Environment.CurrentDirectory, "Data", "PickedMovies.json");
        if (File.Exists(pickedPath))
        {
            var pickedJson = File.ReadAllText(pickedPath);
            if (pickedJson.Length > 0)
            {
                PickedMoviesList = JsonSerializer.Deserialize<List<Movie>>(pickedJson);
            }
        }
        else
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "Data");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Logger.MainLog("Creating New PickedMovies.json");
            File.Create(pickedPath).Dispose();
        }

        Logger.MainLog("Loaded Data");
    }

    public void SaveData()
    {
        var folder = Path.Combine(Environment.CurrentDirectory, "Data");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // remove any duplicate movies by YTVideoId
        UnpickedMoviesList = UnpickedMoviesList.GroupBy(m => m.YTVideoId).Select(g => g.First()).ToList();
        PickedMoviesList = PickedMoviesList.GroupBy(m => m.YTVideoId).Select(g => g.First()).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Save UnpickedMovies to a json file
        var unpickedPath = Path.Combine(Environment.CurrentDirectory, "Data", "UnpickedMovies.json");
        var unpickedJson = JsonSerializer.Serialize(UnpickedMoviesList, options);
        File.WriteAllText(unpickedPath, unpickedJson, Encoding.UTF8);

        // Save PickedMovies to a json file
        var pickedPath = Path.Combine(Environment.CurrentDirectory, "Data", "PickedMovies.json");
        var pickedJson = JsonSerializer.Serialize(PickedMoviesList, options);
        File.WriteAllText(pickedPath, pickedJson, Encoding.UTF8);

        Logger.MainLog("== Saved Data ==", false);
        Logger.MainLog($"{UnpickedMovieFileDetailsString()} Unpicked, {PickedMovieFileDetailsString()} Picked");
    }

    public void ClearCache()
    {
        UnpickedMoviesList.Clear();
        PickedMoviesList.Clear();
        //MoviePosters.Clear();
    }

    public void LoadPosters()
    {
        var imagesDirectory = Path.Combine(Environment.CurrentDirectory, "Data", "MoviePosters");

        if (!Directory.Exists(imagesDirectory))
        {
            Directory.CreateDirectory(imagesDirectory);
        }

        // Load poster files from the directory
        var files = Directory.GetFiles(imagesDirectory);

        foreach (var file in files)
        {
            try
            {
                // if the file is not an image, skip it
                if (!file.EndsWith(".png")) continue;

                var ytId = Path.GetFileNameWithoutExtension(file);
                var poster = new Poster
                {
                    YTVideoid = ytId,
                    Url = file,
                    Image = new BitmapImage(new Uri(file))
                };

               //MoviePosters.Add(poster);
            }
            catch (Exception)
            {
                Logger.MainLog($"Failed to load poster: {file}");
                continue;
            }
        }
    }

    public void SavePoster(Poster poster)
    {
        var imagesDirectory = Path.Combine(Environment.CurrentDirectory, "Data", "MoviePosters");

        if (!Directory.Exists(imagesDirectory))
        {
            Directory.CreateDirectory(imagesDirectory);
        }

        if (poster.Image == null)
        {
            Logger.MainLog($"Poster image is null for: {poster.YTVideoid}");
            return;
        }

        // Save the image to the directory
        //MoviePosters.Add(poster);
        var fileName = $"{poster.YTVideoid}.png";
        var filePath = Path.Combine(imagesDirectory, fileName);

        try
        {
            EnqueueSaveBitmapImageAsPng(poster.Image, filePath);
        }
        catch (Exception ex)
        {
            Logger.MainLog($"Failed to save poster: {poster.YTVideoid}, Exception: {ex.Message}");
        }
    }

    private static Queue<(BitmapImage, string)> _saveQueue = new Queue<(BitmapImage, string)>();
    private static bool _isSaving = false;

    public static void EnqueueSaveBitmapImageAsPng(BitmapImage bitmapImage, string filePath)
    {
        _saveQueue.Enqueue((bitmapImage, filePath));
        if (!_isSaving)
        {
            ProcessSaveQueue();
        }
    }

    public static int SaveDelay = 75; // ms
    private static async void ProcessSaveQueue()
    {
        _isSaving = true;
        while (_saveQueue.Count > 0)
        {
            var (bitmapImage, filePath) = _saveQueue.Dequeue();
            await Task.Run(() => SaveBitmapImageAsPng(bitmapImage, filePath));
            await Task.Delay(SaveDelay); // seems to help loading images
        }
        _isSaving = false;
    }

    static void SaveBitmapImageAsPng(BitmapImage bitmapImage, string filePath)
    {
        bitmapImage.Dispatcher.Invoke(() =>
        {
            // Check if the image is valid
            if (bitmapImage.PixelWidth == 1 || bitmapImage.PixelHeight == 1)
            {
                Logger.MainLog($"Invalid image: {filePath}");
                return;
            }

            // Create a PngBitmapEncoder to encode the image as PNG
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();

            // Create a memory stream to hold the image data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add the BitmapImage to the encoder
                pngEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                // Save the image data to the memory stream
                pngEncoder.Save(memoryStream);

                // Write the memory stream to a file
                File.WriteAllBytes(filePath, memoryStream.ToArray());
            }
        });
    }

    public void AddMovie(Movie movie) // REQUIRES DATA PRELOADED
    {
        if (UnpickedMoviesList.Any(m => m.YTVideoId == movie.YTVideoId) || PickedMoviesList.Any(m => m.YTVideoId == movie.YTVideoId))
        {
            Logger.MainLog($"== Warning ==", false);
            if (Settings.Default.EnableVerboseLogs)
                Logger.MainLog($"Movie already exists in the list: {movie}");
            else
                Logger.MainLog($"Movie already exists in the list: {movie.Title}");
            return;
        }

        UnpickedMoviesList.Add(movie);

        if (Settings.Default.EnableVerboseLogs)
            Logger.MainLog($"Added movie to the list:\n{movie}");
        else
            Logger.MainLog($"Added movie to the list: {movie.Title}");

    }

    public void AddMovies(List<Movie> movies)
    {
        if (movies == null) return;

        foreach (Movie movie in movies)
        {
            AddMovie(movie);
        }
        SaveData();
        
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    public void UpdateMovie(Movie movie)
    {
        var index = UnpickedMoviesList.FindIndex(m => m.YTVideoId == movie.YTVideoId);
        if (index != -1)
        {
            UnpickedMoviesList[index] = movie;

            Logger.MainLog($"Updated movie: {movie.Title}");
        }
    }

    public void UpdateMovies(List<Movie> movies)
    {
        if (movies == null) return;

        foreach (Movie movie in movies)
        {
            UpdateMovie(movie);
        }
        SaveData();

        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI

    }

    // Advanced
    public Movie PickNextMovie()
    {
        if (UnpickedMoviesList.Count == 0)
        {
            Logger.MainLog("No movies to pick from!");
            return null;
        }

        var movie = UnpickedMoviesList[0];
        var removed = UnpickedMoviesList.RemoveAll(m => m.YTVideoId == movie.YTVideoId);
        PickedMoviesList.Add(movie);
        SaveData();

        Logger.MainLog($"Picked movie: {movie.Title}");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
        return movie;
    }

    public void PickMovie(Movie movie)
    {
        // used by the post now button, so may not be next movie. 
        var removed = UnpickedMoviesList.RemoveAll(m => m.YTVideoId == movie.YTVideoId);
        PickedMoviesList.Add(movie);
        SaveData();

        Logger.MainLog($"Picked movie: {movie.Title}");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    // Sorting
    public void SortMoviesByRelevance(string searchTerm)
    {
        UnpickedMoviesList = MovieSorter.SortMoviesByRelevance(UnpickedMoviesList, searchTerm);
        SaveData();

        Settings.Default.SortBy = "Relevance";
        Settings.Default.Save();

        Logger.MainLog($"Sorted movies by relevance: {searchTerm}");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    public void SortMoviesByRating()
    {
        UnpickedMoviesList = MovieSorter.SortMoviesByAverageRating(UnpickedMoviesList);
        SaveData();

        Settings.Default.SortBy = "Rating";
        Settings.Default.Save();

        Logger.MainLog("Sorted movies by rating");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    public void SortMoviesRandomly()
    {
        UnpickedMoviesList = MovieSorter.RandomizeMovies(UnpickedMoviesList);
        SaveData();

        Settings.Default.SortBy = "Randomly";
        Settings.Default.Save();

        Logger.MainLog("Randomized movies");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    public void MoveToFront(Movie movie)
    {
        UnpickedMoviesList.RemoveAll(m => m.YTVideoId == movie.YTVideoId);
        UnpickedMoviesList.Insert(0, movie);
        SaveData();

        Logger.MainLog($"Moved movie to the front of the list: {movie.Title}");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    // Json File Details
    public string PickedMovieFileDetailsString()
    {
        var sb = new StringBuilder();

        sb.Append($"{PickedMoviesList.Count} Movies ~ ");

        var pickedMoviesSize = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Data", "PickedMovies.json")).Length / 1024 / 1024.00;

        sb.Append($"{Math.Round(pickedMoviesSize, 2)}mb");

        return sb.ToString();
    }

    public string UnpickedMovieFileDetailsString()
    {
        var sb = new StringBuilder();

        sb.Append($"{UnpickedMoviesList.Count} Movies ~ ");

        double unpickedMoviesSize = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Data", "UnpickedMovies.json")).Length / 1024 / 1024.00;

        sb.Append($"{Math.Round(unpickedMoviesSize,2)}mb");

        return sb.ToString();
    }

    // Clearing
    public void ClearPickedMovies()
    {
        PickedMoviesList.Clear();
        SaveData();

        Settings.Default.LastClearPickedMoviesDateTime = DateTime.Now;
        Settings.Default.Save();

        Logger.MainLog("Cleared picked movies list");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    public void ClearUnpickedMovies()
    {
        UnpickedMoviesList.Clear();
        SaveData();

        Settings.Default.LastClearUnpickedMoviesDateTime = DateTime.Now;
        Settings.Default.Save();
        
        Logger.MainLog("Cleared unpicked movies list");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }

    internal void DeliverToRear(Movie movie)
    {
        var movieToMove = UnpickedMoviesList.FirstOrDefault(m => m.YTVideoId == movie.YTVideoId);

        if (movieToMove == null) return;

        UnpickedMoviesList.RemoveAll(m => m.YTVideoId == movieToMove.YTVideoId);
        UnpickedMoviesList.Add(movieToMove);
        SaveData();

        Logger.MainLog($"Delivered movie to the rear of the list: {movieToMove.Title}");
        DataChanged?.Invoke(this, new DataChangeArgs()); // update UI
    }
}

// Custom EventArgs, if needed can expand
public class DataChangeArgs : EventArgs
{
    public DataChangeArgs()
    {

    }
}