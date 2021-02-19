using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions
{
    public static class OutcomeExtensions
    {
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