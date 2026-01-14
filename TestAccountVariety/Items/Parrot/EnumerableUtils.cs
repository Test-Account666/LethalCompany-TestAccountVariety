using System.Collections.Generic;
using System.Linq;

namespace TestAccountVariety.Items.Parrot;

public static class EnumerableUtils {
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int times) {
        var enumerable = source as T[] ?? source.ToArray();
        
        for (var i = 0; i < times; i++)
            foreach (var item in enumerable)
                yield return item;
    }
}