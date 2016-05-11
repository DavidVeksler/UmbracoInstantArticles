using System.Linq;
using FEE.Domain.InstantArticles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FEE.StaticTests.InstantArticleTests
{
    [TestClass]
    public class ExtractVideoTagTests
    {
        [TestMethod]
        public void CanGetYouTubeEmbed()
        {
            string src =
                @"<p><iframe width=""100%"" height=""360"" src=""https://www.youtube.com/embed/2fvGasiOkBY"" frameborder=""0"" allowfullscreen=""""></iframe></p>";
            var urls = Html5Utils.GetVideoUrls(src);

            Assert.IsTrue(urls.Any());

        }

        [TestMethod]
        public void CanGetVideoEmbed()
        {
            string src =
                @"<p>  <video>
          <source src=""http://fee-misc.s3.amazonaws.com/video-misc/Isaac%20Moorehouse%20Article%20Intro.mp4"" type=""video/mp4"" />
        </video>
<p>";
            var urls = Html5Utils.GetVideoUrls(src);

            Assert.IsTrue(urls.Any());

        }

    }
}
