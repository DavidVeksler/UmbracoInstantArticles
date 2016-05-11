using FEE.Domain.InstantArticles;
using FEE.Web.App_Code.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbraco.Core.Models;

namespace FEE.StaticTests.InstantArticleTests
{
    [TestClass]
    public class InstantArticleTest
    {
        public void GenerateInstantArticle()
        {
            var article = new Mock<IPublishedContent>() {};
            article.SetupGet(a => a.Name).Returns("TEST");
            article.SetupGet(a => a.Url).Returns("/test/");
            InstantArticleFeedItemModel content = new InstantArticleFeedItemModel(article.Object);
            string html = InstantArticleBuilder.GetArticleBody(content);
        }


        [TestMethod]
        public void ConvertImgToFigure()
        {
            string src = "<img src='hello.jpg'>";

            var wrapped = Html5Utils.WrapImagesInFigure(src);

            Assert.AreEqual(wrapped, "");
        }

        
    }
}