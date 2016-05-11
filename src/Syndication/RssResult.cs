#region

using System.IO;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

#endregion

namespace FEE.Domain.Syndication
{
    public class RssResult : ActionResult
    {
        private readonly SyndicationFeed _feed;
        private readonly IPublishedContent _model;
        public string Format = "atom";

        public RssResult(SyndicationFeed feed, IPublishedContent model)
        {
            _feed = feed;
            _model = model;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            
            if (context.HttpContext.Request.QueryString["format"] != null)
            {
                Format = context.HttpContext.Request.QueryString["format"];
            }

            context.HttpContext.Response.ContentType = "application/rss+xml";

            using (var txtWriter = new Utf8StringWriter())
            {
                var xmlWriter = XmlWriter.Create(txtWriter, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    OmitXmlDeclaration = false
                });

                // Write the Processing Instruction node.
                var xsltHeader = string.Format("type=\"text/xsl\" href=\"{0}\"",
                    _model.UrlWithDomain().EnsureEndsWith('/') + "rss/xslt");
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", xsltHeader);

                SyndicationFeedFormatter formatter;
                if (Format == "rss")
                {
                    formatter = _feed.GetRss20Formatter();
                }
                else
                {
                    formatter = _feed.GetAtom10Formatter();
                }

                formatter.WriteTo(xmlWriter);

                xmlWriter.Flush();

                context.HttpContext.Response.Write(txtWriter.ToString());
            }
        }

        public sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}