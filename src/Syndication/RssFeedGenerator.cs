#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using Umbraco.Cms.Custom;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;

#endregion

namespace FEE.Domain.Syndication
{
    public class RssFeedGenerator : IRssFeedGenerator
    {
        private readonly Regex _relativeMediaHref = new Regex(" href=(?:\"|')(/media/.*?)(?:\"|')",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Regex _relativeMediaSrc = new Regex(" src=(?:\"|')(/media/.*?)(?:\"|')",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public RssFeedGenerator(UmbracoContext umbracoContext)
        {
            
        }

        public SyndicationFeed GetFeed(IPublishedContent rootPageModel, IEnumerable<IArticleSyndicationModel> posts)
        {
            var feed = new SyndicationFeed(
                rootPageModel.GetPropertyValueAsString("pageTitle"),
                rootPageModel.GetPropertyValueAsString("pageDescription"),
                new Uri(rootPageModel.UrlAbsolute()),
                GetFeedItems(rootPageModel, posts))
            {
                Generator = "FEE.org Feeds by DavidV",
                ImageUrl = GetBlogImage(rootPageModel)
            };

            //TODO: attempting to add media:thumbnail...
            //feed.AttributeExtensions.Add(new XmlQualifiedName("media", "http://www.w3.org/2000/xmlns/"), "http://search.yahoo.com/mrss/");

            return feed;
        }

        protected virtual string GetPostContent(IArticleSyndicationModel model)
        {
            return new MvcHtmlString(model.BodyText).ToHtmlString();
        }

        protected virtual SyndicationItem GetFeedItem(IPublishedContent model, IArticleSyndicationModel post,
            string rootUrl)
        {
            var content = _relativeMediaHref.Replace(GetPostContent(post), match =>
            {
                if (match.Groups.Count == 2)
                {
                    return " href=\"" +
                           rootUrl.TrimEnd('/') + match.Groups[1].Value.EnsureStartsWith('/') +
                           "\"";
                }
                return null;
            });
            content = _relativeMediaSrc.Replace(content, match =>
            {
                if (match.Groups.Count == 2)
                {
                    return " src=\"" +
                           rootUrl.TrimEnd('/') + match.Groups[1].Value.EnsureStartsWith('/') +
                           "\"";
                }
                return null;
            });

            var item = new SyndicationItem(
                post.Title,
                new TextSyndicationContent(content, TextSyndicationContentKind.Html),
                new Uri(post.UrlWithDomain()),
                post.Id.ToString(CultureInfo.InvariantCulture),
                post.PublicationDate)
            {
                PublishDate = post.PublicationDate,

                //don't include this as it will override the main content bits
                Summary = new TextSyndicationContent(post.Description)
        };
            

            //FeaturedImageUrl:
            if (!post.FeaturedImageUrl.IsNullOrWhiteSpace())
            {
                item.ElementExtensions.Add(
                    new XElement("enclosure",
                        new XAttribute("type", "image/jpeg"),
                        new XAttribute("url", new Uri(post.FeaturedImageUrl))
                        ).CreateReader()
                    );
            }

            if (post.AttachedFile != null)
            {
                var file = post.AttachedFile;
                var mimetype = GetImageType(file.GetPropertyValue<string>("umbracoExtension", ""));

                item.ElementExtensions.Add(
                    new XElement("enclosure",
                        new XAttribute("type", mimetype),
                        new XAttribute("url", new Uri("http://fee.org" + file.Url)),
                        new XAttribute("length", file.GetPropertyValue("umbracoBytes", ""))
                        ).CreateReader()
                    );
            }

            //TODO: attempting to add media:thumbnail...
            //item.ElementExtensions.Add(new SyndicationElementExtension("thumbnail", "http://search.yahoo.com/mrss/", "This is a test!"));

            foreach (var c in post.Categories)
            {
                item.Categories.Add(new SyndicationCategory(c));
            }

            return item;
        }

        private IEnumerable<SyndicationItem> GetFeedItems(IPublishedContent model,
            IEnumerable<IArticleSyndicationModel> posts)
        {
            var rootUrl = model.UrlWithDomain();
            return posts.Select(post => GetFeedItem(model, post, rootUrl)).ToList();
        }

        protected virtual Uri GetBlogImage(IPublishedContent rootPageModel)
        {
            Uri logoUri = null;
            try
            {
                var featuredImage = ContentHelper.GetFeaturedImage(rootPageModel);
                logoUri = featuredImage != null
                    ? null
                    : new Uri(rootPageModel.UrlAbsolute());
            }
            catch (Exception ex)
            {
                LogHelper.Error<RssFeedGenerator>("Could not convert the blog logo path to a Uri", ex);
            }
            return logoUri;
        }

        /// <summary>
        ///     Get the current mimetype for extension
        /// </summary>
        /// <returns></returns>
        public string GetImageType(string extension)
        {
            switch (extension)
            {
                case "png":
                    return "image/png";

                case "jpg":
                case "jpeg":
                    return "image/jpeg";

                case "pdf":
                    return "application/pdf";

                case "mp3":
                    return "audio/mpeg3";

                case "mp4":
                case "avi":
                    return "video/mpeg";
                case "epub":
                    return "application/epub+zip";
                default:
                    return "application/" + extension;
            }
        }
    }
}