using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FEE.Domain;
using FEE.Domain.InstantArticles;
using FEE.Domain.People;
using FEE.Domain.Syndication;
using FEE.InstantArticles;
using Umbraco.Cms.Custom;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace FEE.Web.App_Code.Controllers
{
    public class InstantArticleFeedItemModel : PublishedContentWrapped, IArticleSyndicationModel
    {
        public int Id
        {
            get { return base.Id; }

        }
        public string UrlWithDomain()
        {
            return this.UrlAbsolute();
        }

        public InstantArticleFeedItemModel(IPublishedContent content) : base(content)
        {
        }

        public string Title
        {
            get
            {
                var value = Content.GetPropertyValue<string>("articleTitle", true);
                //if (value.IsNullOrWhiteSpace()) // commented out because all the freeman issues have "The Freeman" as the title
                //{
                //    value = Content.GetPropertyValue<string>("pageTitle", true);
                //}
                if (value.IsNullOrWhiteSpace())
                {
                    value = Content.Name;
                }
                return value;
            }
        }

        public string Description
        {
            get
            {
                var subtitle = Content.GetPropertyValue<string>("subtitle", true);
                var @abstract = Content.GetPropertyValue<string>("abstract", true);

                return !@abstract.IsNullOrWhiteSpace()
                    ? @abstract
                    : !subtitle.IsNullOrWhiteSpace() ? subtitle : Content.GetPropertyValue<string>("pageTitle", true);
            }
        }

        public List<string> Authors
        {
            get
            {
                var value = ContentHelper.GetAuthors(Content.GetPropertyValue("authors", true));
                return value.Select(a => a.Name).ToList();
            }
        }

        public List<string> Categories
        {
            get
            {
                var topicsString = Content.GetPropertyValueAsString("topics");
                if (string.IsNullOrWhiteSpace(topicsString))
                {
                    return new List<string>();
                }
                var topics = topicsString.Split(',').Select(int.Parse);
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var topicsNodes = umbracoHelper.TypedContent(topics);
                return topicsNodes.Select(a => a.Name).ToList();
            }
        }

        public string BodyText
        {
            get { return InstantArticleBuilder.GetArticleBody(this); }
        }

        public DateTime PublicationDate
        {
            get
            {
                return Content.GetPropertyValueAsDateTime("publicationDate") != DateTime.MinValue
                    ? Content.GetPropertyValueAsDateTime("publicationDate")
                    : Content.CreateDate;
            }
        }


        public string FeaturedImageUrl
        {
            get
            {
                try
                {
                    var featured = ContentHelper.GetFeaturedImage(this);
                    if (featured != null)
                    {
                        return "https://fee.org" + featured.Url;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ArticleSyndicationModel>("FeaturedImageUrl", ex);
                }
                return string.Empty;
            }
        }

        public IPublishedContent AttachedFile
        {
            get
            {
                try
                {
                    var featured = ContentHelper.GetFeaturedImage(this, "attachedFile");
                    return featured;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<Domain.Syndication.ArticleSyndicationModel>("AttachedFile", ex);
                }
                return null;
            }
        }
    }
}
