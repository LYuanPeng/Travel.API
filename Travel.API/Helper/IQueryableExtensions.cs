using System.Collections.Generic;
using System.Linq;
using Travel.API.Services;
using System.Linq.Dynamic.Core;
using System;

namespace Travel.API.Helper
{
    public static class IQueryableExtensions 
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source,
            string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                return source;
            }

            var orderByString = string.Empty;

            var orderByAfterSplit = orderBy.Split(',');

            foreach (var order in orderByAfterSplit)
            {
                var trimmedOrder = order.Trim();

                // 通过字符串“desc”来判断升序还是降序
                var orderDescending = trimmedOrder.EndsWith(" desc");

                // 删除升序或降序字符串“ asc” or “ desc”来判断升序还是降序
                var indexOfFirstSpace = trimmedOrder.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1
                    ? trimmedOrder
                    : trimmedOrder.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentNullException($"Key mapping for {propertyName} is missing.");
                }

                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                foreach (var destinationProperty in propertyMappingValue.DestinationProperty.Reverse())
                {
                    // 给IQueryable 添加排序字符串
                    orderByString = orderByString +
                        (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                        + destinationProperty
                        + (orderDescending ? " descending" : " ascending");
                }
            }

            return source.OrderBy(orderByString);
        }
    }
}
