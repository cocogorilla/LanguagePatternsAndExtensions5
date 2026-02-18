using System;

namespace LanguagePatternsAndExtensions;

public static class OptionExtensions
{
    public static Option<T> ToOption<T>(this T? item) where T : notnull
    {
        return item is not null ? Option<T>.Some(item) : Option<T>.None();
    }

    extension<TSource>(Option<TSource> source) where TSource : notnull
    {
        public Option<TResult> SelectMany<TResult>(Func<TSource, Option<TResult>> selector) where TResult : notnull
        {
            return source.Match(
                some: selector,
                nothing: Option<TResult>.None());
        }

        public Option<TResult> SelectMany<TIntermediate, TResult>(Func<TSource, Option<TIntermediate>> intermediate,
            Func<TSource, TIntermediate, TResult> selector) where TIntermediate : notnull
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

        public Option<TResult> Select<TResult>(Func<TSource, TResult> selector) where TResult : notnull
        {
            return source.Match(
                some: x => selector(x).ToOption(),
                nothing: Option<TResult>.None());
        }

        public Option<TResult> Map<TResult>(Func<TSource, TResult> selector) where TResult : notnull
        {
            return source.Select(selector);
        }

        public Option<TSource> Where(Func<TSource, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return source.Match(
                some: x => predicate(x) ? source : Option<TSource>.None(),
                nothing: Option<TSource>.None());
        }

        public Option<TSource> Tap(Action<TSource> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            source.IfSome(action);
            return source;
        }
    }

    public static Option<T> ToOption<T>(this T? item) where T : struct
    {
        return item?.ToOption() ?? Option<T>.None();
    }
}