﻿using System;
using Espera.Core.Audio;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using Rareform.Validation;

namespace Espera.Core
{
    public sealed class YoutubeSongFinder : SongFinder<YoutubeSong>
    {
        private const string ApiKey =
            "AI39si5_zcffmO_ErRSZ9xUkfy_XxPZLWuxTOzI_1RH9HhXDI-GaaQ-j6MONkl2JiF01yBDgBFPbC8-mn6U9Qo4Ek50nKcqH5g";

        private readonly string searchString;

        /// <summary>
        /// Initializes a new instance of the <see cref="YoutubeSongFinder"/> class.
        /// </summary>
        /// <param name="searchString">The search string.</param>
        public YoutubeSongFinder(string searchString)
        {
            if (searchString == null)
                Throw.ArgumentNullException(() => searchString);

            this.searchString = searchString;
        }

        /// <summary>
        /// Starts the <see cref="YoutubeSongFinder"/>.
        /// </summary>
        public override void Start()
        {
            var query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri)
                            {
                                OrderBy = "relevance",
                                Query = searchString,
                                SafeSearch = YouTubeQuery.SafeSearchValues.None
                            };

            var settings = new YouTubeRequestSettings("Espera", ApiKey);
            var request = new YouTubeRequest(settings);
            Feed<Video> feed = request.Get<Video>(query);

            foreach (Video video in feed.Entries)
            {
                var duration = TimeSpan.FromSeconds(Int32.Parse(video.YouTubeEntry.Duration.Seconds));
                string url = video.WatchPage.OriginalString
                    .Replace("&feature=youtube_gdata_player", String.Empty) /* Unnecessary long url */
                    .Replace("https://", "http://"); /* VLC doesn't like https */

                var song = new YoutubeSong(url, AudioType.Mp3, duration, CoreSettings.Default.StreamYoutube)
                               {
                                   Title = video.Title,
                                   Description = video.Description,
                                   Rating = video.RatingAverage,
                                   ThumbnailSource = new Uri(video.Thumbnails[0].Url)
                               };

                this.InternSongsFound.Add(song);

                this.OnSongFound(new SongEventArgs(song));
            }

            this.OnFinished(EventArgs.Empty);
        }
    }
}