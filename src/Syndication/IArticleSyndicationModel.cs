using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FEE.Domain.Syndication
{
    public interface IArticleSyndicationModel
    {
        int Id { get; }
        string UrlWithDomain();

        IPublishedContent AttachedFile { get; }
        List<string> Authors { get; }
        string BodyText { get; }
        List<string> Categories { get; }
        string Description { get; }
        string FeaturedImageUrl { get; }
        DateTime PublicationDate { get; }
        string Title { get; }
        
    }
}