using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Shmap.Common.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }

        return collection;
    }
}