using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic.Devices;
using YouTubeMoviePicker.Models;
using YouTubeMoviePicker.Services;

namespace YouTubeMoviePicker.Controls;
/// <summary>
/// Interaction logic for MovieListItem.xaml
/// </summary>
public partial class MovieListItem : UserControl
{
    public Movie Movie { get; set; }
    public Poster Poster { get; set; }

    public EventHandler ItemClicked;
    public EventHandler ItemDoubleClicked;

    public MovieListItem(Movie movie, string num = "")
    {
        InitializeComponent();

        Movie = movie;
        Index.Content = num;

        // Set initial UI values
        Title.Text = movie.Title;
        Genre.Text = movie.Genre;
        ReleaseDate.Text = movie.Released;
        Rating.Text = movie.Rated;

        // Set placeholder image
        Thumbnail.Source = new BitmapImage(new Uri("/Resources/MissingImage.png", UriKind.Relative));

        // on loaded event 
        this.Loaded += MovieListItem_Loaded;
    }

    private void MovieListItem_Loaded(object sender, RoutedEventArgs e)
    {
        // setup poster
        Poster = FindImageInCache(Movie.YTVideoId);
        if (Poster != null)
        {
            Thumbnail.Source = Poster.Image;
        }
        else
        {
            FetchMoviePoster(Movie);
        }
    }

    public Poster FindImageInCache(string ytId)
    {
        return DataService.Instance.MoviePosters.FirstOrDefault(p => p.YTVideoid == ytId);
    }

    public async void FetchMoviePoster(Movie movie)
    {
        // prioritize Omdb poster over YouTube thumbnail
        try
        {
            Poster = await Fetch(movie);
            await Dispatcher.InvokeAsync(() => Thumbnail.Source = Poster.Image);
            DataService.Instance.SavePoster(Poster);
        }
        catch (Exception)
        {
            await Dispatcher.InvokeAsync(() => Thumbnail.Source = new BitmapImage(new Uri("/Resources/MissingImage.png", UriKind.Relative)));
        }
    }

    private async Task<Poster> Fetch(Movie movie)
    {
        try
        {
            // try to fetch poster from omdb
            var posterUri = new Uri(movie.Poster);
            var httpClient = new HttpClient();
            var stream = await httpClient.GetStreamAsync(posterUri);

            var posterImage = new BitmapImage();

            posterImage.BeginInit();
            posterImage.StreamSource = stream;
            posterImage.CacheOption = BitmapCacheOption.OnLoad;
            posterImage.EndInit();

            if (posterImage.Width == 1)
            {
                Logger.MainLog($"Poster image download Failed: {movie.YTVideoId} - {movie.Title}");
            }

            var poster = new Poster()
            {
                YTVideoid = movie.YTVideoId,
                Url = movie.Poster,
                Image = posterImage
            };

            return poster;
        }
        catch (Exception ex)
        {
            try
            {
                // try to fetch the poster from the YouTube thumbnail
                var posterUri = new Uri(movie.YTVideoThumbnail);
                var httpClient = new HttpClient();
                var stream = await httpClient.GetStreamAsync(posterUri);
                var posterImage = new BitmapImage();

                posterImage.BeginInit();
                posterImage.StreamSource = stream;
                posterImage.CacheOption = BitmapCacheOption.OnLoad;
                posterImage.EndInit();

                var poster = new Poster()
                {
                    YTVideoid = movie.YTVideoId,
                    Url = movie.YTVideoThumbnail,
                    Image = posterImage
                };

                return poster;
            }
            catch (Exception ex2)
            {
                Logger.MainLog($"Error == Failed to fetch poster image:\n{movie.Poster}\n{ex}\n{movie.YTVideoThumbnail}\n{ex2}");
                throw new Exception($"Error == Failed to fetch poster image:\n{movie.Poster}\n{movie.YTVideoThumbnail}\n {ex}\n{ex2}");
            }
        }
    }

    public async void Clicked(object sender, RoutedEventArgs e)
    {
        ItemClicked?.Invoke(this, e);
        GradColor.Color = Color.FromArgb(60, 100, 114, 128);
        await Task.Delay(100);

        // if mouse is still over the item, change the color back
        if (System.Windows.Input.Mouse.DirectlyOver == this)
            GradColor.Color = Color.FromArgb(60, 39, 42, 45);
        else
            GradColor.Color = Color.FromArgb(60, 13, 75, 136);
    }

    public void DoubleClicked(object sender, RoutedEventArgs e)
    {
        DataService.Instance.MoveToFront(this.Movie);
    }

    public void OnMouseEnter(object sender, MouseEventArgs e)
    {
        GradColor.Color = Color.FromArgb(60,13,75,136);
    }

    public void OnMouseLeave(object sender, MouseEventArgs e)
    {
        GradColor.Color = Color.FromArgb(60, 39,42,45);
    }
}
