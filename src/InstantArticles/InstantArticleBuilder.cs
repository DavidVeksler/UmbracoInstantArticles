using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FEE.Domain.People;
using FEE.Domain.SocialSharing;
using FEE.Domain.UmbracoHelpers;
using FEE.Web.App_Code.Controllers;
using Umbraco.Cms.Custom;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace FEE.Domain.InstantArticles
{
    public class InstantArticleBuilder
    {
        private readonly string _pageId = ConfigurationManager.AppSettings["FacebookPageId"];
        private readonly string _accessToken = ConfigurationManager.AppSettings["FacebookPageAccessToken"];

    
        //https://developers.facebook.com/docs/instant-articles/api
        public string PushArticleToFacebook(IContent content)
        {
            try
            {
                var publishedContent = new UmbracoHelper(UmbracoContext.Current).TypedContent(content.Id);

                var article = new InstantArticleFeedItemModel(publishedContent);

                var htmlSource = GetArticleBody(article);

                var graphUrl = string.Format("https://graph.facebook.com/{0}/instant_articles", _pageId);

                using (var client = new WebClientWithTimeout())
                {
                    var response =
                        client.UploadValues(graphUrl, new NameValueCollection
                        {
                            {"access_token", _accessToken},
                            {"published", "true"},
                            {"development_mode", "false"},
                            {"html_source", htmlSource}
                        });

                    var result = Encoding.UTF8.GetString(response);
                    Debug.WriteLine(result);
                    return result;
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var error = reader.ReadToEnd();
                    Debug.WriteLine(error);
                    Console.WriteLine(error);
                    LogHelper.Error<InstantArticleBuilder>("WebException:", ex);
                    return error;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<InstantArticleBuilder>("Exception:", ex);
                Debug.WriteLine(ex);
                return ex.Message;
            }
        }


        public static string GetArticleBody(InstantArticleFeedItemModel Content)
        {
            //< !--template: https://www.facebook.com/feeonline/publishing_tools/?section=INSTANT_ARTICLES_SAMPLE-->
            var bodyText = Content.GetPropertyValue<string>("bodyText", true);

            try
            {
                var pattern = @"(?<=<span class=""quote"">)(?<content>.+?)(?=</span>)";
                bodyText = Regex.Replace(bodyText, pattern, m => "<aside>" + m.Groups["content"].Value + "</aside>");

                bodyText = Html5Utils.MakeUrlsAbsolute(bodyText);

                // needs more testing
                //bodyText = HtmlUtils.SanitizeHtml(bodyText);

                bodyText = Html5Utils.WrapImagesInFigure(bodyText);

                bodyText = Html5Utils.WrapSocialInFigure(bodyText);

                bodyText = Html5Utils.RemoveFiguresFromParagraph(bodyText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            var contents = new StringBuilder(FeeDomainResources.InstantArticleBody);

            

            contents.Replace("{body}", bodyText);

            contents.Replace("{title}", Content.Title);

            string videoUrl = VideoHelper.ExtractMediaFileUrlFromVideoEmbeds(Content);
            
            const string imgTag = @"<img src=""{img}"" />";
            const string videoTag = @"<video><source src=""{img}"" type=""video/mp4"" /></video>";

            contents.Replace("{img}", !string.IsNullOrWhiteSpace(videoUrl) ? videoTag.Replace("{img}", videoUrl) : imgTag.Replace("{img}", Content.FeaturedImageUrl));

            contents.Replace("{subtitle}", Content.Description.StripHtml());


            contents.Replace("{pubdate}", Content.PublicationDate.ToLongDateString());
            contents.Replace("{pubdateISO}", Content.PublicationDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));


            contents.Replace("{modified}", Content.UpdateDate.ToLongDateString());
            ;
            contents.Replace("{modifiedISO}", Content.UpdateDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            contents.Replace("{url}", Content.UrlWithDomain());
            contents.Replace("{topic}", Content["topic"]?.ToString() ?? string.Empty);

            var authorTemplate = @"
            <address>
                <a title=""{title}"">{author}</a>                
            </address>
            ";
            var authorsStr = string.Empty;

            var authors = ContentHelper.GetAuthors(Content["authors"]);
            if (authors.Any())
            {
                foreach (var author in authors)
                {
                    var authorModel = AuthorModelBuilder.GetAuthorModel(author);
                    var a = new StringBuilder(authorTemplate);
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
}