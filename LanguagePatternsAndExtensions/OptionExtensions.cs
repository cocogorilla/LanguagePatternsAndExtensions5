using System;

namespace LanguagePatternsAndExtensions;

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
            selector,
            Option<TResult>.None());
    }

    public static Option<TResult> SelectMany<TSource, TIntermediate, TResult>(
        this Option<TSource> source,
        Func<TSource, Option<TIntermediate>> intermediate,
        Func<TSource, TIntermediate, TResult> selector)
    {
        return source.Match(
            x =>
            {
                var elem = intermediate(x);
                return elem.Match(
                    y => selector(x, y).ToOption(),
                    Option<TResult>.None());
            },
            Option<TResult>.None());
    }

    public static Option<T> ToOption<T>(this T? item) where T : struct
    {
        return item?.ToOption() ?? Option<T>.None();
    }
}