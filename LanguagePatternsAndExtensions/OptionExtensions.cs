using System;

namespace LanguagePatternsAndExtensions
{
    public static class OptionExtensions
    {
        public static Option<T> ToOption<T>(this T item)
        {
            return Option<T>.Some(item);
        }

        public static Option<TResult> SelectMany<TSource, TResult>(
            this Option<TSource> source,
            Func<TSource, Option<TResult>> selector)
        {
            return source.Match(
                Option<TResult>.None(),
                selector);
        }

        public static Option<TResult> SelectMany<TSource, TIntermediate, TResult>(
            this Option<TSource> source,
            Func<TSource, Option<TIntermediate>> intermediate,
            Func<TSource, TIntermediate, TResult> selector)
        {
            return source.Match(
                Option<TResult>.None(),
                x =>
                {
                    var elem = intermediate(x);
                    return elem.Match(
                        Option<TResult>.None(),
                        y => selector(x, y).ToOption());
                });
        }

        public static Option<T> ToOption<T>(this T? item) where T : struct
        {
            return item?.ToOption() ?? Option<T>.None();
        }
    }
}