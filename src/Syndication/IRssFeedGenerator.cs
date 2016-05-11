#region

using System.Collections.Generic;
using System.ServiceModel.Syndication;
using Umbraco.Core.Models;

#endregion

namespace FEE.Domain.Syndication
{
    public interface IRssFeedGenerator
    {
        SyndicationFeed GetFeed(IPublishedContent rootPageModel, IEnumerable<IArticleSyndicationModel> posts);
    }
}