namespace SG.Generator
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class GroupingExtensionMethods
    {
        public static void Deconstruct<TGroup, TItem>(this IGrouping<TGroup, TItem> grouping, out TGroup group, out IEnumerable<TItem> items)
        {
            group = grouping.Key;
            items = grouping;
        }
    }
}
