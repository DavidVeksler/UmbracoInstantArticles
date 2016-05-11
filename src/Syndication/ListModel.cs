using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace FEE.Domain.Syndication
{
    /// <summary>
    ///     Represents a page that displays a list of blog posts
    /// </summary>
    public class ListModel : PublishedContentWrapped
    {
        private readonly bool _disableSort;
        private readonly IEnumerable<IPublishedContent> _listItems;
        private IEnumerable<ArticleSyndicationModel> _resolvedList;

        /// <summary>
        ///     Constructor accepting an explicit list of child items
        /// </summary>
        /// <param name="content"></param>
        /// <param name="listItems"></param>
        /// <remarks>
        ///     Default sorting by published date will be disabled for this list model, it is assumed that the list items will
        ///     already be sorted.
        /// </remarks>
        public ListModel(IPublishedContent content, IEnumerable<IPublishedContent> listItems)
            : base(content)
        {
            if (listItems == null) throw new ArgumentNullException("listItems");
            _disableSort = true;
            _listItems = listItems;
        }

        public ListModel(IPublishedContent content)
            : base(content)
        {
            _listItems = base.Children;
        }


        /// <summary>
        ///     The list of blog posts
        /// </summary>
        public override IEnumerable<IPublishedContent> Children
        {
            get
            {
                if (_resolvedList != null)
                {
                    return _resolvedList;
                }

                if (_listItems == null)
                {
                    //we'll get the result from the base children
                    _resolvedList = base.Children
                        .Select(x => new ArticleSyndicationModel(x));
                    if (!_disableSort)
                    {
                        _resolvedList = _resolvedList.OrderByDescending(x => x.PublicationDate);
                    }
                    _resolvedList = _resolvedList.ToArray();
                }
                else
                {
                    _resolvedList = _listItems
                        .Select(x => new ArticleSyndicationModel(x));
                    if (!_disableSort)
                    {
                        _resolvedList = _resolvedList.OrderByDescending(x => x.PublicationDate);
                    }
                    _resolvedList = _resolvedList.ToArray();
                }

                return _resolvedList;
            }
        }
    }
}