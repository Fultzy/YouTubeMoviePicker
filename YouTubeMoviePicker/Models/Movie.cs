using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeMoviePicker.Models;
public class Movie
{
    public string? Title { get; set; }

    // OMbd API properties
    public string? Genre { get; set; }
    public string? Year { get; set; }
    public string? Released { get; set; }
    public string? Plot { get; set; }
    public string? Poster { get; set; }
    public string? Rated { get; set; }
    public string? Runtime { get; set; }
    public string? Director { get; set; }
    public string? Actors { get; set; }
    public string? Writer { get; set; }
    public string? Awards { get; set; }
    public string? BoxOffice { get; set; }
    public string? Production { get; set; }
    public string? Metascore { get; set; }
    public string? imdbId { get; set; }
    public string? imdbRating { get; set; }
    public string? imdbVotes { get; set; }

    // YouTube API properties
    public string? YTVideoId { get; set; }
    public string? YTChannelTitle { get; set; }
    public string? YTChannelId { get; set; }
    public string? YTDescription { get; set; }
    public string? YTVideoThumbnail { get; set; }
    public string? YTVideoURL { get; set; }
    public string? YTVideoPublishedAt { get; set; }

    internal void AddOMdbDetails(Movie movie)
    {
        Genre = movie.Genre;
        Year = movie.Year;
        Released = movie.Released;
        Plot = movie.Plot?.Replace('"', '`');
        Poster = movie.Poster;
        Rated = movie.Rated;
        Runtime = movie.Runtime;
        Director = movie.Director;
        Actors = movie.Actors;
        Writer = movie.Writer;
        Awards = movie.Awards;
        BoxOffice = movie.BoxOffice;
        Production = movie.Production;
        imdbId = movie.imdbId;
        Metascore = movie.Metascore;
        imdbRating = movie.imdbRating;
        imdbVotes = movie.imdbVotes;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(Title)) sb.Append($"Title: {Title}");
        if (!string.IsNullOrEmpty(Genre)) sb.Append($"\nGenres: {Genre}");
        if (!string.IsNullOrEmpty(Year)) sb.Append($"\nYear: {Year}");
        if (!string.IsNullOrEmpty(Released)) sb.Append($"\nRelease Date: {Released}");
        if (!string.IsNullOrEmpty(Plot)) sb.Append($"\nDescription: {Plot}");
        if (!string.IsNullOrEmpty(Poster)) sb.Append($"\nImage Path: {Poster}");
        if (!string.IsNullOrEmpty(Rated)) sb.Append($"\nRating: {Rated}");
        if (!string.IsNullOrEmpty(Runtime)) sb.Append($"\nDuration: {Runtime}");
        if (!string.IsNullOrEmpty(Director)) sb.Append($"\nDirectors: {Director}");
        if (!string.IsNullOrEmpty(Actors)) sb.Append($"\nActors: {Actors}");
        if (!string.IsNullOrEmpty(Writer)) sb.Append($"\nWriters: {Writer}");
        if (!string.IsNullOrEmpty(Awards)) sb.Append($"\nAwards: {Awards}");
        if (!string.IsNullOrEmpty(BoxOffice)) sb.Append($"\nBox Office: {BoxOffice}");
        if (!string.IsNullOrEmpty(Production)) sb.Append($"\nProduction: {Production}");
        if (!string.IsNullOrEmpty(imdbId)) sb.Append($"\nIMdb ID: {imdbId}");
        if (!string.IsNullOrEmpty(YTVideoId)) sb.Append($"\nYouTube Video ID: {YTVideoId}");
        if (!string.IsNullOrEmpty(YTChannelTitle)) sb.Append($"\nYouTube Channel Title: {YTChannelTitle}");
        if (!string.IsNullOrEmpty(YTChannelId)) sb.Append($"\nYouTube Channel ID: {YTChannelId}");
        if (!string.IsNullOrEmpty(YTDescription)) sb.Append($"\nYouTube Description: {YTDescription}");
        if (!string.IsNullOrEmpty(YTVideoThumbnail)) sb.Append($"\nYouTube Thumbnail: {YTVideoThumbnail}");
        if (!string.IsNullOrEmpty(YTVideoURL)) sb.Append($"\nYouTube Video URL: {YTVideoURL}");
        if (!string.IsNullOrEmpty(YTVideoPublishedAt)) sb.Append($"\nYouTube Published At: {YTVideoPublishedAt}");

        return sb.ToString();
    }
}
