using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FEE.Domain;
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
            get
            {
                //< !--template: https://www.facebook.com/feeonline/publishing_tools/?section=INSTANT_ARTICLES_SAMPLE-->
                string bodyText = Content.GetPropertyValue<string>("bodyText", true);

                try
                {
                    string pattern = @"(?<=<span class=""quote"">)(?<content>.+?)(?=</span>)";
                    bodyText = Regex.Replace(bodyText, pattern, m => "<aside>" + m.Groups["content"].Value + "</aside>");

                    bodyText = HtmlUtils.MakeUrlsAbsolute(bodyText);
                    
                    // needs more testing
                    //bodyText = HtmlUtils.SanitizeHtml(bodyText);

                    bodyText = HtmlUtils.WrapImagesInFigure(bodyText);


                    bodyText = HtmlUtils.WrapSocialInFigure(bodyText);

                    bodyText = HtmlUtils.RemoveFiguresFromParagraph(bodyText);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

               

                //<figure data-feedback="fb:likes, fb:comments">



                StringBuilder contents = new StringBuilder(FeeDomainResources.InstantArticleBody);

                /*<figure>
                    <img src="@UrlUtils.GetExternalUrl(instantArticleImage.Image)" class="instant-article"/>
                    <figcaption>@Html.Raw(instantArticleImage.ImageCaption)</figcaption>
                </figure>*/

                contents.Replace("{body}", bodyText);

                //contents.Replace("<span class=\"quote\">", "<aside>");

                contents.Replace("{title}", Title);
                contents.Replace("{img}", FeaturedImageUrl);
                contents.Replace("{subtitle}", this.Description.StripHtml());


                contents.Replace("{pubdate}", this.PublicationDate.ToLongDateString());
                contents.Replace("{pubdateISO}", this.PublicationDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));


                contents.Replace("{modified}", this.UpdateDate.ToLongDateString()); ;
                contents.Replace("{modifiedISO}", this.UpdateDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                contents.Replace("{url}", this.UrlWithDomain());
                contents.Replace("{topic}", this["topic"]?.ToString() ?? string.Empty);

                string authorTemplate = @"
            <address>
                <a title=""{title}"">{author}</a>
                <!-- {bio} -->
            </address>
";
                string authorsStr = "";

                List<IPublishedContent> authors = ContentHelper.GetAuthors(this["authors"]);
                if (authors.Any())
                {
                    foreach (IPublishedContent author in authors)
                    {
                        var authorModel = AuthorModelBuilder.GetAuthorModel(author);
                        StringBuilder a = new StringBuilder(authorTemplate);
                        a.Replace("{author}", authorModel.FullName);
                        a.Replace("{bio}", authorModel.BriefBio);
                        a.Replace("{title}", authorModel.Title);

                        authorsStr += a.ToString();
                    }
                }

                contents.Replace("{authors}", authorsStr);

                return contents.ToString();

            }
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
                    LogHelper.Error<Domain.Syndication.ArticleSyndicationModel>("FeaturedImageUrl", ex);
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
