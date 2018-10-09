using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceGovernance.Registry
{
    /// <summary>
    /// Extension methods for all kinds of lists
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Determines whether the given selector exists more than once in the given list.
        /// </summary>
        /// <typeparam name="T">Type of the list item.</typeparam>
        /// <typeparam name="TProp">Type of the selector result.</typeparam>
        /// <param name="list">The list to check for duplicates.</param>
        /// <param name="selector">The function to define the selector.</param>
        /// <returns></returns>
        public static bool HasDuplicates<T, TProp>(this IEnumerable<T> list, Func<T, TProp> selector)
        {
            var d = new HashSet<TProp>();
            foreach (var t in list)
            {
                if (!d.Add(selector(t)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
