using System;

namespace LanguagePatternsAndExtensions;

public static class OptionExtensions
{
    public static Option<T> ToOption<T>(this T? item) where T : notnull
    {
        return Option<T>.Some(item);
    }

    public static Option<TResult> SelectMany<TSource, TResult>(
        this Option<TSource> source,
        Func<TSource, Option<TResult>> selector)
        where TSource : notnull
        where TResult : notnull
    {
        return source.Match(
            some: selector,
            nothing: Option<TResult>.None());
    }

    public static Option<TResult> SelectMany<TSource, TIntermediate, TResult>(
        this Option<TSource> source,
        Func<TSource, Option<TIntermediate>> intermediate,
        Func<TSource, TIntermediate, TResult> selector)
        where TSource : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        return source.Match(
            some: x =>
            {
                var elem = intermediate(x);
                return elem.Match(
                    some: y => selector(x, y).ToOption(),
                    nothing: Option<TResult>.None());
            },
            nothing: Option<TResult>.None());
    }

    public static Option<T> ToOption<T>(this T? item) where T : struct
    {
        return item?.ToOption() ?? Option<T>.None();
    }
}