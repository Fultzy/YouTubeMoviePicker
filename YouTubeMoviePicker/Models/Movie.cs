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
        Plot = movie.Plot;
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

    ////////////////////////////////////
    public string DiscordPayload()
    {
        var sb = new StringBuilder();

        //sb.Append("\n__                                                              __");
        sb.Append("\n:popcorn::movie_camera:*Today's Movie is ... :movie_camera::popcorn:*");

        if (!string.IsNullOrEmpty(Title)) 
            sb.Append($"\n      **{Title}**");

        if (!string.IsNullOrEmpty(Year))
            sb.Append($" *- {Year}*");

        if (!string.IsNullOrEmpty(Rated)) 
            sb.Append($"\n      *Rated {Rated}    {Genre}*");
        
        if (!string.IsNullOrEmpty(Plot))
        {
            sb.Append($"\n\n     **{Plot}**\n");
        }
        else // if no plot from OMdb, use YTDescription
        {
            sb.Append($"\n\n     **{YTDescription}**");
        }

        if (!string.IsNullOrEmpty(Actors) && Actors != "N/A")
            sb.Append($"\n*:person::skin-tone-4:  Cast:*  **{Actors}**");

        if (!string.IsNullOrEmpty(Director) && Director != "N/A")
            sb.Append($"\n*:projector: Directed by:* **{Director}**");

        if (!string.IsNullOrEmpty(Writer) && Writer != "N/A")            
            sb.Append($"\n*:writing_hand::skin-tone-4: Written by:*  **{Writer}**");

        if (!string.IsNullOrEmpty(Released) || !string.IsNullOrEmpty(Production) || !string.IsNullOrEmpty(BoxOffice) || !string.IsNullOrEmpty(Awards))
        {
            sb.Append("\n__      __"); // Separator if needed
        }

        if (!string.IsNullOrEmpty(Released) && Released != "N/A")
            sb.Append($"\n*Released:*  **{Released}**");

        if (!string.IsNullOrEmpty(Production) && Production != "N/A")
            sb.Append($"\n*Production:*  **{Production}**");

        if (!string.IsNullOrEmpty(BoxOffice) && BoxOffice != "N/A")
            sb.Append($"\n*Box Office:*  **{BoxOffice}**");

        if (!string.IsNullOrEmpty(Awards) && Awards != "N/A")
            sb.Append($"\n*Awards:*  **{Awards}**");

        // determine ratings
        if (!string.IsNullOrEmpty(Metascore) || !string.IsNullOrEmpty(imdbRating) || !string.IsNullOrEmpty(imdbVotes))
        {
            sb.Append("\n");
        }

        if (!string.IsNullOrEmpty(Metascore) && Metascore != "N/A")
            sb.Append($"*Metascore* **{Metascore}/100**     ");

        if (!string.IsNullOrEmpty(imdbRating) && imdbRating != "N/A")
            sb.Append($"*IMdb Rating* **{imdbRating}/10** ");

        if (!string.IsNullOrEmpty(imdbVotes) && imdbVotes != "N/A")
            sb.Append($"*- Votes* **{imdbVotes}**");


        sb.Append("\n__      __"); // Separator 

        // determine disclaimer
        var disclaimer = "\nEnjoy the movie! :popcorn:";

        if (string.IsNullOrEmpty(Plot) && !string.IsNullOrEmpty(YTDescription))
            disclaimer = "\nEnjoy the movie! :popcorn: (no additional details available..)";

        if (!string.IsNullOrEmpty(Plot))
            disclaimer = "\n*Movie details may sometimes be inaccurate..* :person_shrugging::skin-tone-4: ";

        sb.Append(disclaimer);

        if (!string.IsNullOrEmpty(YTVideoURL)) 
            sb.Append($"\n{YTVideoURL}");

        return sb.ToString();
    }

    public string SlackPayload()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(Year))
            sb.Append($"{Year}");

        if (!string.IsNullOrEmpty(Rated))
            sb.Append($" - Rated {Rated}");

        if (!string.IsNullOrEmpty(Genre))
            sb.Append($" - {Genre}");

        if (!string.IsNullOrEmpty(Plot))
        {
            sb.Append($"\n\n     {Plot}\n");
        }
        else // if no plot from OMdb, use YTDescription
        {
            sb.Append($"\n\n     {YTDescription}");
        }

        if (!string.IsNullOrEmpty(Actors) && Actors != "N/A")
            sb.Append($"\n:person_in_tuxedo::skin-tone-4:  Cast:  {Actors}");

        if (!string.IsNullOrEmpty(Director) && Director != "N/A")
            sb.Append($"\n:film_projector: Directed by: {Director}");

        if (!string.IsNullOrEmpty(Writer) && Writer != "N/A")
            sb.Append($"\n:writing_hand: Written by:  {Writer}");

        if (!string.IsNullOrEmpty(Released) || !string.IsNullOrEmpty(Production) || !string.IsNullOrEmpty(BoxOffice) || !string.IsNullOrEmpty(Awards))
        {
            sb.Append("\n"); // Separator if needed
        }

        if (!string.IsNullOrEmpty(Released) && Released != "N/A")
            sb.Append($"\nReleased:  {Released}");

        if (!string.IsNullOrEmpty(Production) && Production != "N/A")
            sb.Append($"\nProduction:  {Production}");

        if (!string.IsNullOrEmpty(BoxOffice) && BoxOffice != "N/A")
            sb.Append($"\nBox Office:  {BoxOffice}");

        if (!string.IsNullOrEmpty(Awards) && Awards != "N/A")
            sb.Append($"\nAwards:  {Awards}");

        // determine ratings
        if (!string.IsNullOrEmpty(Metascore) || !string.IsNullOrEmpty(imdbRating) || !string.IsNullOrEmpty(imdbVotes))
        {
            sb.Append("\n");
        }

        if (!string.IsNullOrEmpty(Metascore) && Metascore != "N/A")
            sb.Append($"Metascore: {Metascore}/100     ");

        if (!string.IsNullOrEmpty(imdbRating) && imdbRating != "N/A")
            sb.Append($"IMdb Rating: {imdbRating}/10 ");

        if (!string.IsNullOrEmpty(imdbVotes) && imdbVotes != "N/A")
            sb.Append($"- Votes {imdbVotes}");


        sb.Append("\n"); // Separator 

        // determine disclaimer
        var disclaimer = "\nEnjoy the movie! :popcorn:";

        if (string.IsNullOrEmpty(Plot) && !string.IsNullOrEmpty(YTDescription))
            disclaimer = "\nEnjoy the movie! :popcorn: (no additional details available..)";

        if (!string.IsNullOrEmpty(Plot))
            disclaimer = "\nMovie details may sometimes be inaccurate.. :shrug:";

        sb.Append(disclaimer);

        return sb.ToString();
    }
}
