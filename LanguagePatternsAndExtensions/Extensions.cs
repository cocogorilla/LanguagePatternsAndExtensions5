using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions;

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

    extension<T>(IEnumerable<T> enumerable)
    {
        public T RandomElement()
        {
            return enumerable.RandomElementUsing<T>(new Random());
        }

        public T RandomElementUsing(Random rand)
        {
            var index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }
    }

    extension(IEnumerable @this)
    {
        public void Iter(Action<object> act)
        {
            foreach (var item in @this)
            {
                act(item);
            }
        }

        public void Iter(Action<object, int> act)
        {
            var counter = 0;
            foreach (var item in @this)
            {
                act(item, counter++);
            }
        }

        public async Task IterAsync(Func<object, Task> act)
        {
            foreach (var item in @this)
            {
                await act(item);
            }
        }

        public async Task IterAsync(Func<object, int, Task> act)
        {
            var counter = 0;
            foreach (var item in @this)
            {
                await act(item, counter++);
            }
        }
    }

    extension<T>(IEnumerable<T> items)
    {
        public void Iter(Action<T> act)
        {
            foreach (var item in items) act(item);
        }

        public void Iter(Action<T, int> act)
        {
            int counter = 0;
            foreach (var item in items)
            {
                act(item, counter);
                counter++;
            }
        }

        public async Task IterAsync(Func<T, Task> act)
        {
            foreach (var item in items)
            {
                await act(item);
            }
        }

        public async Task IterAsync(Func<T, int, Task> act)
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