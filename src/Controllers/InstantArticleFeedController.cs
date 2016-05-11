using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using FEE.Domain.Freeman;
using FEE.Domain.Syndication;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace FEE.Web.App_Code.Controllers
{
    // todo: inherit from InstantArticleRssPageController
    public class InstantArticleFeedController : SurfaceController
    {
        protected IRssFeedGenerator FeedGenerator
        {
            get { return new RssFeedGenerator(UmbracoContext); }
        }

        [HttpGet]
        public ActionResult RenderFeed(int maxItems = 25)
        {
            var umb = new UmbracoHelper(UmbracoContext.Current);

            var articles = new FeatureQuery(umb).GetArticleArchive();
            var rootPageModel = umb.TypedContent(1124);

            SyndicationFeed feed = FeedGenerator.GetFeed(rootPageModel,
                articles.Take(maxItems).Select(x => new InstantArticleFeedItemModel(x)));

            return new RssResult(feed, rootPageModel) {Format = "rss"};
        }

        /// <summary>
        ///     Returns the XSLT to render the RSS nicely in a browser
        /// </summary>
        /// <returns></returns>
        public ActionResult FeedXslt()
        {
            var result = FEEResources.FeedXslt;
            return Content(result, "text/xml");
        }
    }
}