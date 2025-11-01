using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media;
using YouTubeMoviePicker.Services;
using YouTubeMoviePicker.Utility;

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
    public List<Rating>? Ratings { get; set; }


    // YouTube API properties
    public string? YTVideoId { get; set; }
    public string? YTChannelTitle { get; set; }
    public string? YTChannelId { get; set; }
    public string? YTDescription { get; set; }
    public string? YTVideoThumbnail { get; set; }
    public string? YTVideoURL { get; set; }
    public string? YTVideoPublishedAt { get; set; }

    public void AddOMdbDetails(JsonObject omdbObject)
    {
        if (omdbObject == null) return;

        FromJson.Object(omdbObject, "Genre", out string genreValue);
        FromJson.Object(omdbObject, "Year", out string yearValue);
        FromJson.Object(omdbObject, "Released", out string releasedValue);
        FromJson.Object(omdbObject, "Plot", out string plotValue);
        FromJson.Object(omdbObject, "Poster", out string posterValue);
        FromJson.Object(omdbObject, "Rated", out string ratedValue);
        FromJson.Object(omdbObject, "Runtime", out string runtimeValue);
        FromJson.Object(omdbObject, "Director", out string directorValue);
        FromJson.Object(omdbObject, "Actors", out string actorsValue);
        FromJson.Object(omdbObject, "Writer", out string writerValue);
        FromJson.Object(omdbObject, "Awards", out string awardsValue);
        FromJson.Object(omdbObject, "BoxOffice", out string boxOfficeValue);
        FromJson.Object(omdbObject, "Production", out string productionValue);
        FromJson.Object(omdbObject, "imdbID", out string imdbIdValue);
        FromJson.Object(omdbObject, "imdbRating", out string imdbRatingValue);
        FromJson.Object(omdbObject, "imdbVotes", out string imdbVotesValue);
        FromJson.Object(omdbObject, "Metascore", out string metascoreValue);

        Genre = genreValue;
        Year = yearValue;
        Released = releasedValue;
        Plot = plotValue;
        Poster = posterValue;
        Rated = ratedValue;
        Runtime = runtimeValue;
        Director = directorValue;
        Actors = actorsValue;
        Writer = writerValue;
        Awards = awardsValue;
        BoxOffice = boxOfficeValue;
        Production = productionValue;
        imdbId = imdbIdValue;
        imdbRating = imdbRatingValue;
        imdbVotes = imdbVotesValue;
        Metascore = metascoreValue;

        var ratingsArray = omdbObject["Ratings"]?.AsArray();
        var ratings = new List<Rating>();
        if (ratingsArray != null)
        {
            foreach (var ratingNode in ratingsArray)
            {
                var rating = new Rating
                {
                    Source = ratingNode["Source"]?.ToString(),
                    Value = ratingNode["Value"]?.ToString()
                };
                if (rating.Source == "Internet Movie Database") rating.Source = "IMDb";
                ratings.Add(rating);
            }
        }
        Ratings = ratings;
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


public class Rating
{
    public string? Source { get; set; }
    public string? Value { get; set; }
}