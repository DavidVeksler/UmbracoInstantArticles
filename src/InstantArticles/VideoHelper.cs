using System.Linq;
using Umbraco.Cms.Custom;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace FEE.Domain.InstantArticles
{
    public static class VideoHelper
    {
        private const string VideoPropertyName = "featuredVideo";

        /// <summary>
        /// Extra video url from youtube and html5 video embeds
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ExtractUrlFromVideoEmbeds(IPublishedContent content)
        {
            return YouTubeEmbedUrlToViewUrl(GetVideoFromContent(content));
        }

        public static string ExtractMediaFileUrlFromVideoEmbeds(IPublishedContent content)
        {
            string videoUrl = YouTubeEmbedUrlToViewUrl(GetVideoFromContent(content));

            if (videoUrl != null && (videoUrl.EndsWith(".mp4") || videoUrl.EndsWith(".m4v")))
            {
                videoUrl = null;
            }

            return videoUrl;

        }

        public static string YouTubeEmbedUrlToViewUrl(this string url)
        {
            return url.Replace("youtube.com/embed/", "youtube.com/v/").Replace("http:", "https:");
        }

        internal static string GetVideoFromContent(IPublishedContent content)
        {
            var helper = new UmbracoHelper(UmbracoContext.Current);

            if (content.HasProperty(VideoPropertyName) &&
                !string.IsNullOrWhiteSpace(content.GetPropertyValueAsString(VideoPropertyName)))
            {
                var media = helper.TypedMedia(content.GetPropertyValue(VideoPropertyName));

                return (string)media["src"];
            }

            if (content.HasProperty("bodyText") &&
                !string.IsNullOrWhiteSpace(content.GetPropertyValueAsString("bodyText")))
            {
                string[] urls = Html5Utils.GetVideoUrls(content.GetPropertyValueAsString("bodyText"));

                if (urls != null && urls.Any())
                {
                    return urls[0];
                }
            }

            return string.Empty;
        }


    }
}