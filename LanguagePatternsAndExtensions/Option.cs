using System;

namespace LanguagePatternsAndExtensions
{
    public struct Option<T>
    {
        private readonly T _item;

        public Option(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _item = item;
            IsSome = true;
        }

        public Option(Unit none)
        {
            _item = default;
            IsSome = false;
        }

        public bool IsSome { get; }
        public bool IsNone => !IsSome;

        public static Option<T> Some(T source)
        {
            return source == null
                ? None()
                : new Option<T>(source);
        }

        public static Option<T> None()
        {
            return new Option<T>(Unit.Default);
        }

        public TResult Match<TResult>(TResult nothing, Func<T, TResult> some)
        {
            if (nothing == null) throw new ArgumentNullException(nameof(nothing));
            if (some == null) throw new ArgumentNullException(nameof(some));

            return (IsSome)
                ? some(_item)
                : nothing;
        }

        public TResult Match<TResult>(Func<TResult> nothing, Func<T, TResult> some)
        {
            if (nothing == null) throw new ArgumentNullException(nameof(nothing));
            if (some == null) throw new ArgumentNullException(nameof(some));
            return (IsSome)
                ? some(_item)
                : nothing();
        }

        /// <summary>
        /// Unsafe, directly retrieve a value assuming it is not null and apply a func transform
        /// Example: var theValue = optional.GetValue(x => x);
        /// theValue may be null
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        public TResult GetValue<TResult>(Func<T, TResult> transform)
        {
            return transform(_item);
        }
    }
}