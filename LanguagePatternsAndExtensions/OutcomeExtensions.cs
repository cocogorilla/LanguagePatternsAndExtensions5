using System;

namespace LanguagePatternsAndExtensions
{
    public static class OutcomeExtensions
    {
        public static Outcome<TResult> Select<TSource, TResult>(
            this Outcome<TSource> outcome,
            Func<TSource, TResult> selector)
        {
            return outcome
                .Traverse(
                    source => Success.Of(selector(source)),
                    Failure.Nok<TResult>);
        }

        public static Outcome<TResult> SelectMany<TSource, TResult>(
            this Outcome<TSource> source,
            Func<TSource, Outcome<TResult>> selector)
        {
            return source
                .Traverse(
                    selector,
                    Failure.Nok<TResult>);
        }

        public static Outcome<TResult> SelectMany<TSource, TIntermediate, TResult>(
            this Outcome<TSource> source,
            Func<TSource, Outcome<TIntermediate>> intermediate,
            Func<TSource, TIntermediate, TResult> selector)
        {
            return source.Traverse(
                x =>
                {
                    var elem = intermediate(x);
                    return elem.Traverse(
                        y => Success.Of(selector(x, y)),
                        Failure.Nok<TResult>);
                },
                Failure.Nok<TResult>);
        }
    }
}