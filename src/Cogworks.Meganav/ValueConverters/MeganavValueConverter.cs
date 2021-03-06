﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cogworks.Meganav.Enums;
using Cogworks.Meganav.Models;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;

namespace Cogworks.Meganav.ValueConverters
{
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    [PropertyValueType(typeof(IEnumerable<MeganavItem>))]
    public class MeganavValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<MeganavItem>>(source.ToString());

                return BuildMenu(items);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MeganavValueConverter>("Failed to convert Meganav", ex);
            }

            return null;
        }

        internal IEnumerable<MeganavItem> BuildMenu(IEnumerable<MeganavItem> items, int level = 0)
        {
            items = items.ToList();

            foreach (var item in items)
            {
                item.Level = level;

                // it's likely a content item
                if (item.Id > 0)
                {
                    item.ItemType = ItemType.Content;
                    item.Content = UmbracoContext.Current.ContentCache.GetById(item.Id);
                }

                // process child items
                if (item.Children.Any())
                {
                    var childLevel = item.Level + 1;

                    BuildMenu(item.Children, childLevel);
                }
            }

            // remove unpublished content items
            items = items.Where(x => x.Content != null || x.ItemType == ItemType.Link);

            return items;
        }
    }
}