using System;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions;

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

    public static async Task<Outcome<TResult>> Select<TSource, TResult>(
        this Task<Outcome<TSource>> outcomeTask,
        Func<TSource, TResult> selector)
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return outcome.Select(selector);
    }

    public static async Task<Outcome<TResult>> SelectMany<TSource, TResult>(
        this Task<Outcome<TSource>> outcomeTask,
        Func<TSource, Task<Outcome<TResult>>> binder)
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return await outcome.Traverse(
            binder,
            error => Task.FromResult(Failure.Nok<TResult>(error))).ConfigureAwait(false);
    }

    public static async Task<Outcome<TResult>> SelectMany<TSource, TIntermediate, TResult>(
        this Task<Outcome<TSource>> outcomeTask,
        Func<TSource, Task<Outcome<TIntermediate>>> binder,
        Func<TSource, TIntermediate, TResult> projector)
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return await outcome.Traverse(
            async x =>
            {
                var intermediateOutcome = await binder(x).ConfigureAwait(false);
                return intermediateOutcome.Traverse(
                    y => Success.Of(projector(x, y)),
                    Failure.Nok<TResult>);
            },
            error => Task.FromResult(new Outcome<TResult>(error))
        ).ConfigureAwait(false);
    }
}