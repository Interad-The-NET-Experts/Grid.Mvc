﻿using System;
using System.Globalization;
using System.Linq;
using GridMvc.Core.Utility;
using Microsoft.AspNetCore.Http;

namespace GridMvc.Core.Pagination
{
    /// <summary>
    ///     Default grid pager implementation
    /// </summary>
    public class GridPager : IGridPager
    {
        public const int DefaultMaxDisplayedPages = 5;
        public const int DefaultPageSize = 20;

        public const string DefaultPageQueryParameter = "grid-page";
        public const string DefaultPagerViewName = "_GridPager";

        private readonly HttpContext _context;
        private readonly CustomQueryStringBuilder _queryBuilder;
        private int _currentPage;

        private int _itemsCount;
        private int _maxDisplayedPages;
        private int _pageSize;

        #region ctor's

        public GridPager(HttpContext context)
        {
            if (context == null)
                throw new Exception("No http context here!");

            _context = context;
            _currentPage = -1;
            _queryBuilder = new CustomQueryStringBuilder(context.Request.Query.ToNameValueCollection());

            ParameterName = DefaultPageQueryParameter;
            TemplateName = DefaultPagerViewName;
            MaxDisplayedPages = MaxDisplayedPages;
            PageSize = DefaultPageSize;
        }

        #endregion

        #region IGridPager members

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                RecalculatePages();
            }
        }

        public int CurrentPage
        {
            get
            {
                if (_currentPage >= 0) return _currentPage;
                _currentPage = GetCurrentPage(_context, ParameterName);
                if (_currentPage > PageCount)
                    _currentPage = PageCount;
                return _currentPage;
            }
            protected internal set
            {
                _currentPage = value;
                if (_currentPage > PageCount)
                    _currentPage = PageCount;
                RecalculatePages();
            }
        }

        public static int GetCurrentPage(HttpContext context, string parameterName = null)
        {
            if (string.IsNullOrEmpty(parameterName))
                parameterName = DefaultPageQueryParameter;

            var currentPageString = context.Request.Query[parameterName].FirstOrDefault() ?? "1";
            if (!int.TryParse(currentPageString, out var currentPage))
                currentPage = 1;

            return currentPage;
        }

        #endregion

        /// <summary>
        ///     Query string parameter name, that determine current displaying page
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        ///     Total items of the initial collection
        /// </summary>
        public virtual int ItemsCount
        {
            get { return _itemsCount; }
            set
            {
                _itemsCount = value;
                RecalculatePages();
            }
        }

        public int MaxDisplayedPages
        {
            get { return _maxDisplayedPages == 0 ? DefaultMaxDisplayedPages : _maxDisplayedPages; }
            set
            {
                _maxDisplayedPages = value;
                RecalculatePages();
            }
        }

        /// <summary>
        ///     Total pages count
        /// </summary>
        public int PageCount { get; protected set; }

        public virtual void Initialize<T>(IQueryable<T> items)
        {
            if (ItemsCountOverwrite.HasValue)
                ItemsCount = ItemsCountOverwrite.Value;
            else
                ItemsCount = items.Count(); //take total items count from collection
        }

        protected virtual void RecalculatePages()
        {
            if (ItemsCount == 0)
            {
                PageCount = 0;
                return;
            }
            PageCount = (int) (Math.Ceiling(ItemsCount/(double) PageSize));

            //if (CurrentPage > PageCount)
            //    CurrentPage = PageCount;

            StartDisplayedPage = (CurrentPage - MaxDisplayedPages/2) < 1 ? 1 : CurrentPage - MaxDisplayedPages/2;
            EndDisplayedPage = (CurrentPage + MaxDisplayedPages/2) > PageCount
                                   ? PageCount
                                   : CurrentPage + MaxDisplayedPages/2;
        }

        #region View

        public int StartDisplayedPage { get; protected set; }
        public int EndDisplayedPage { get; protected set; }
        public string TemplateName { get; set; }
        public int? ItemsCountOverwrite { get; set; }

        public virtual string GetLinkForPage(int pageIndex)
        {
            return _queryBuilder.GetQueryStringWithParameter(ParameterName,
                                                             pageIndex.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}