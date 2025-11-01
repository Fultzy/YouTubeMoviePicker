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
        FetchMoviePoster(Movie);
    }

    public async void FetchMoviePoster(Movie movie)
    {
        // prioritize Omdb poster over YouTube thumbnail
        try
        {
            Thumbnail.Source = await PosterService.Fetch(movie);
        }
        catch (Exception)
        {
            await Dispatcher.InvokeAsync(() => Thumbnail.Source = new BitmapImage(new Uri("/Resources/MissingImage.png", UriKind.Relative)));
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
