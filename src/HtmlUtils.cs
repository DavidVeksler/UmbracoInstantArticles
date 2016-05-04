using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ServiceStack.Common.Extensions;
using System.Web;

namespace FEE.InstantArticles
{
    public static class HtmlUtils
    {
        private static readonly Dictionary<string, string[]> ValidHtmlTags =
            new Dictionary<string, string[]>
            {
                {"p", new string[] {}},
                {"div", new[] {"*"}},
                {"h1", new string[] {}},
                {"h2", new string[] {}},
                {"h3", new string[] {}},
                {"h4", new string[] {}},
                {"h5", new string[] {}},
                {"h6", new string[] {}},
                {"ol", new string[] {}},
                {"ul", new string[] {}},
                {"li", new string[] {}},
                {"blockquote", new string[] {}},
                {"a", new[] {"href"}},
                {"cite", new string[] {}},
                {"aside", new string[] {}},
                {"figure", new[] {"*"}},
                {"img", new string[] {}}
            };



        /// <summary>
        ///     Takes raw HTML input and cleans against a whitelist
        /// </summary>
        /// <param name="source">Html source</param>
        /// <returns>Clean output</returns>
        internal static string SanitizeHtml(string source)
        {
            //source = RewriteUrlsToFriendly(source, true);

            if (source == null)
                return null;
            source = source.Replace("<p>&nbsp;</p>", "");
            source = source.Replace("<p></p>", "");

            HtmlDocument html = GetHtml(source);
            if (html == null) return string.Empty;

            // All the nodes
            HtmlNode allNodes = html.DocumentNode;

            // Select whitelist tag names
            var whitelist = (from kv in ValidHtmlTags
                             select kv.Key).ToArray();

            // Scrub tags not in whitelist
            CleanNodes(allNodes, whitelist);

            // Filter the attributes of the remaining
            foreach (var tag in ValidHtmlTags)
            {
                IEnumerable<HtmlNode> nodes = from n in allNodes.DescendantsAndSelf()
                                              where n.Name == tag.Key
                                              select n;

                if (nodes == null) continue;

                foreach (var n in nodes)
                {
                    if (!n.HasAttributes) continue;

                    // Get all the allowed attributes for this tag
                    HtmlAttribute[] attr = n.Attributes.ToArray();


                    foreach (HtmlAttribute a in attr)
                    {
                        if (tag.Value.Contains("*")) continue;
                        if (!tag.Value.Contains(a.Name))
                        {
                            a.Remove(); // Wasn't in the list
                        }
                        //else
                        //{
                        //    // AntiXss
                        //    a.Value =
                        //        Microsoft.Security.Application.Encoder.UrlPathEncode(a.Value);
                        //}
                    }
                }
            }
            RemoveNestedParagraphElements(allNodes);


            return allNodes.InnerHtml;
        }


        /// <summary>
        ///     Recursively delete nodes not in the whitelist
        /// </summary>
        private static void CleanNodes(HtmlNode node, string[] whitelist)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (!whitelist.Contains(node.Name))
                {
                    node.ParentNode.RemoveChild(node);
                    return; // We're done
                }
            }

            if (node.HasChildNodes)
                CleanChildren(node, whitelist);
        }

        /// <summary>
        ///     Apply CleanNodes to each of the child nodes
        /// </summary>
        private static void CleanChildren(HtmlNode parent, string[] whitelist)
        {
            for (var i = parent.ChildNodes.Count - 1; i >= 0; i--)
                CleanNodes(parent.ChildNodes[i], whitelist);
        }

        /// <summary>
        ///     Helper function that returns an HTML document from text
        /// </summary>
        private static HtmlDocument GetHtml(string source)
        {
            HtmlDocument html = new HtmlDocument();
            html.OptionFixNestedTags = true;
            html.OptionAutoCloseOnEnd = true;
            html.OptionDefaultStreamEncoding = Encoding.UTF8;

            html.LoadHtml(source);

            return html;
        }

        /// <summary>
        ///     Removed nested <p></p>-elements
        /// </summary>
        /// <param name="allNodes"></param>
        private static void RemoveNestedParagraphElements(HtmlNode allNodes)
        {
            var pTags = allNodes.SelectNodes("//p");
            if (pTags != null)
            {
                foreach (var tag in pTags)
                {
                    if (tag.InnerText.IsNullOrEmpty())
                    {
                        tag.ParentNode.RemoveChild(tag, true);
                    }
                }
            }
        }

        public static string MakeUrlsAbsolute(string source)
        {
            HtmlDocument html = GetHtml(source);
            if (html == null) return string.Empty;

            // All the nodes
            HtmlNode allNodes = html.DocumentNode;
            var nodes = allNodes.SelectNodes("//a");
            if (nodes != null)
            {
                foreach (HtmlNode link in nodes)
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value.StartsWith("/"))
                    {
                        var url = att.Value;
                        string host = "https://" + umbraco.library.RequestServerVariables("SERVER_NAME");
                        url = host + url;

                        att.Value = url;
                    }
                }
            }

            nodes = allNodes.SelectNodes("//img");
            if (nodes != null)
            {
                foreach (HtmlNode link in nodes)
                {
                    HtmlAttribute att = link.Attributes["src"];
                    if (att.Value.StartsWith("/"))
                    {
                        var url = att.Value;

                        string host = "https://" + umbraco.library.RequestServerVariables("SERVER_NAME");
                        url = host + url;

                        att.Value = url;
                    }
                }
            }

            return allNodes.InnerHtml;
        }


        //    var toExternal = new FriendlyHtmlRewriteToExternal(UrlBuilder.RebaseKind.ToRootRelative);
        //    input = Regex.Replace(input, "<a(.*?)href=\"/link/*\"", "<a$1href=\"" + EPiServer.Configuration.Settings.Instance.SiteUrl + Global.UrlRewriteProvider.ConvertToExternal(urlBuilder, null, System.Text.Encoding.UTF8), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //    input = Regex.Replace(input, "<img(.*?)src=\"/link/*\"", "<img$1src=\"" + EPiServer.Configuration.Settings.Instance.SiteUrl, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //    // Make url's absolute
        //    Global.UrlRewriteProvider.ConvertToExternal(urlBuilder, null, System.Text.Encoding.UTF8);
        //    var urlBuilder = new UrlBuilder("");
        //{
        //public static string RewriteUrlsToAbsolut(string input)

        //    return toExternal.RewriteString(
        //        new UrlBuilder(HttpContext.Current.Request.Path),
        //        new UrlBuilder(HttpContext.Current.Request.RawUrl),
        //        HttpContext.Current.Response.ContentEncoding,
        //        input);
        //}

        //https://developers.facebook.com/docs/instant-articles/reference/image
        public static string WrapImagesInFigure(string source)
        {
            HtmlDocument html = GetHtml(source);
            if (html == null) return string.Empty;

            // All the nodes
            HtmlNode allNodes = html.DocumentNode;
            var nodes = allNodes.SelectNodes("//img");
            if (nodes == null)
                return source;

            foreach (HtmlNode link in nodes)
            {
                HtmlAttribute src = link.Attributes["src"];
                if (!string.IsNullOrWhiteSpace(src?.Value))
                {
                    string newNodeStr = @"<figure data-feedback=""fb: likes, fb: comments""><img src=""" + src.Value + @"""/><figure>";

                    var newNode = HtmlNode.CreateNode(newNodeStr);
                    link.ParentNode.ReplaceChild(newNode, link);
                }

            }
            string final = allNodes.InnerHtml;
            
            //bug:
            final = final.Replace("<figure></figure>", string.Empty);

            return final;
        }

        // https://developers.facebook.com/docs/instant-articles/reference/social
        public static string WrapSocialInFigure(string source)
        {
            HtmlDocument html = GetHtml(source);
            if (html == null) return string.Empty;

            // All the nodes
            HtmlNode allNodes = html.DocumentNode;
            var nodes = allNodes.SelectNodes("//iframe");
            if (nodes == null)
                return source;

            foreach (HtmlNode link in nodes)
            {
                string outerHtml = link.OuterHtml;

                if (!string.IsNullOrWhiteSpace(outerHtml))
                {
                    string newNodeStr = @"<figure class=""op-social"">{0}</figure>";

                    newNodeStr = string.Format(newNodeStr, outerHtml);

                    var newNode = HtmlNode.CreateNode(newNodeStr);

                    link.ParentNode.ReplaceChild(newNode, link);
                }

            }
            string final = allNodes.InnerHtml;

            //bug:
            final = final.Replace("<figure></figure>", string.Empty);

            return final;
        }

        public static string RemoveFiguresFromParagraph(string source)
        {
            HtmlDocument html = GetHtml(source);

            if (html == null) return string.Empty;

            var document = html.DocumentNode;

            var figures = document.SelectNodes("//figure");
            if(figures == null )
            {
                return source;
            }

            foreach(HtmlNode figure in figures)
            {
                HtmlNode parent = figure.ParentNode;
                if(parent.Name.ToLower() == "p")
                {
                    parent.ParentNode.InsertBefore(figure, parent);
                    parent.RemoveChild(figure);
                }
            }

            string final = document.InnerHtml;
            final = final.Replace("<p></p>", "");

            return final;
        }
    }


    //    var toExternal = new FriendlyHtmlRewriteToExternal(UrlBuilder.RebaseKind.ToRootRelative);
    //    input = Regex.Replace(input, "<a(.*?)href=\"/link/*\"", "<a$1href=\"" + EPiServer.Configuration.Settings.Instance.SiteUrl + Global.UrlRewriteProvider.ConvertToExternal(urlBuilder, null, System.Text.Encoding.UTF8), RegexOptions.IgnoreCase | RegexOptions.Compiled);
    //    input = Regex.Replace(input, "<img(.*?)src=\"/link/*\"", "<img$1src=\"" + EPiServer.Configuration.Settings.Instance.SiteUrl, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    //    // Make url's absolute
    //    Global.UrlRewriteProvider.ConvertToExternal(urlBuilder, null, System.Text.Encoding.UTF8);
    //    var urlBuilder = new UrlBuilder("");
    //{
    //public static string RewriteUrlsToAbsolut(string input)

    //    return toExternal.RewriteString(
    //        new UrlBuilder(HttpContext.Current.Request.Path),
    //        new UrlBuilder(HttpContext.Current.Request.RawUrl),
    //        HttpContext.Current.Response.ContentEncoding,
    //        input);
    //}
}
