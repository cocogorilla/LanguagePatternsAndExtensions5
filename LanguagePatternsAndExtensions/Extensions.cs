using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions
{
    public static class Extensions
    {
        public static IReadOnlyCollection<T> AsReadonlyCollection<T>(this IEnumerable<T> @this)
        {
            return @this.ToList().AsReadOnly();
        }

        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            if (item != null)
                yield return item;
        }

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.RandomElementUsing<T>(new Random());
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, Random rand)
        {
            var index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static void Iter(this IEnumerable @this, Action<object> act)
        {
            foreach (var item in @this)
            {
                act(item);
            }
        }

        public static void Iter(this IEnumerable @this, Action<object, int> act)
        {
            var counter = 0;
            foreach (var item in @this)
            {
                act(item, counter++);
            }
        }

        public static async Task IterAsync(this IEnumerable @this, Func<object, Task> act)
        {
            foreach (var item in @this)
            {
                await act(item);
            }
        }

        public static async Task IterAsync(this IEnumerable @this, Func<object, int, Task> act)
        {
            var counter = 0;
            foreach (var item in @this)
            {
                await act(item, counter++);
            }
        }

        public static void Iter<T>(this IEnumerable<T> items, Action<T> act)
        {
            foreach (var item in items) act(item);
        }

        public static void Iter<T>(this IEnumerable<T> items, Action<T, int> act)
        {
            int counter = 0;
            foreach (var item in items)
            {
                act(item, counter);
                counter++;
            }
        }

        public static async Task IterAsync<T>(this IEnumerable<T> items, Func<T, Task> act)
        {
            foreach (var item in items)
            {
                await act(item);
            }
        }

        public static async Task IterAsync<T>(this IEnumerable<T> items, Func<T, int, Task> act)
        {
            int counter = 0;
            foreach (var item in items)
            {
                await act(item, counter);
                counter++;
            }
        }
    }
}
