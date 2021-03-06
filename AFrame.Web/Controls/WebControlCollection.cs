﻿using AFrame.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AFrame.Web.Controls
{
    public class WebControlCollection<T> : ControlCollection<T> where T : WebControl
    {
        public new WebContext Context { get { return base.Context as WebContext; } }

        public WebControlCollection(WebContext context, IEnumerable<SearchProperty> searchProperties)
            : base(context, Technology.Web, searchProperties)
        { }

        protected override object RawFind()
        {
            var allElements = new List<T>();

            //Find the absolute selector.
            var absSelector = Helpers.ToAbsoluteSelector(this.Context.SearchPropertyStack);

            var selectorParts = absSelector.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            foreach (var selector in selectorParts)
	        {
		        //Get all the elements.
                var elements = Helpers.JQueryFindElements(this.Context, selector);
                var strToFormat = selector + ":eq({0})";
                for (int i = 0; i < elements.Count(); i++)
                {
                    var indexedSelector = string.Format(strToFormat, i);

                    //Add all search parameters bar the jquery selector.
                    var searchParameters = this.Context.SearchPropertyStack.Last().Where(x => x.Name != WebControl.SearchNames.JQuerySelector).ToList();
                   
                    //Add the new indexed jquery selector.
                    searchParameters.Add(new SearchProperty(WebControl.SearchNames.JQuerySelector, indexedSelector));
                    
                    allElements.Add(this.CreateControlItem<T>(searchParameters));
                }
	        }
           
            return allElements;
        }

        protected override T2 CreateControlItem<T2>(IEnumerable<SearchProperty> searchProperties)
        {
            var wrapperSearchProperties = new SearchPropertyStack();
            wrapperSearchProperties.Add(searchProperties);

            //Each time we create a control, we add the selector of its parent.
            var newContext = new WebContext(this.Context.Driver, this.Context.ParentContext /*Parent Context*/, wrapperSearchProperties);
            return (T2)Activator.CreateInstance(typeof(T2), newContext);
        }
    }
}
