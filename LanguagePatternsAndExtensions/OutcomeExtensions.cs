using System;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions;

/// <summary>
/// Provides extension methods for working with Outcome objects in a functional style.
/// </summary>
public static class OutcomeExtensions
{
    public static Outcome<TResult> Select<TSource, TResult>(
        this Outcome<TSource> outcome,
        Func<TSource, TResult> selector)
    {
        return outcome
            .Match(
                source => Success.Of(selector(source)),
                Failure.Nok<TResult>);
    }

    public static Outcome<TResult> SelectMany<TSource, TResult>(
        this Outcome<TSource> source,
        Func<TSource, Outcome<TResult>> selector)
    {
        return source
            .Match(
                selector,
                Failure.Nok<TResult>);
    }

    public static Outcome<TResult> SelectMany<TSource, TIntermediate, TResult>(
        this Outcome<TSource> source,
        Func<TSource, Outcome<TIntermediate>> intermediate,
        Func<TSource, TIntermediate, TResult> selector)
    {
        return source.Match(
            x =>
            {
                var elem = intermediate(x);
                return elem.Match(
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
        return await outcome.Match(
            binder,
            error => Task.FromResult(Failure.Nok<TResult>(error))).ConfigureAwait(false);
    }

    public static async Task<Outcome<TResult>> SelectMany<TSource, TIntermediate, TResult>(
        this Task<Outcome<TSource>> outcomeTask,
        Func<TSource, Task<Outcome<TIntermediate>>> binder,
        Func<TSource, TIntermediate, TResult> projector)
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return await outcome.Match(
            async x =>
            {
                var intermediateOutcome = await binder(x).ConfigureAwait(false);
                return intermediateOutcome.Match(
                    y => Success.Of(projector(x, y)),
                    Failure.Nok<TResult>);
            },
            error => Task.FromResult(Outcome<TResult>.Failure(error))
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms the success value of an Outcome using the provided selector function.
    /// Does not execute if the Outcome represents a failure.
    /// </summary>
    /// <typeparam name="TSuccess">The original success type.</typeparam>
    /// <typeparam name="TFailure">The failure type.</typeparam>
    /// <typeparam name="TResult">The result type after transformation.</typeparam>
    /// <param name="outcome">The source Outcome.</param>
    /// <param name="selector">The function to transform the success value.</param>
    /// <returns>A new Outcome with the transformed value or the original error.</returns>
    /// <example>
    /// var result = Outcome&lt;int, string&gt;.Success(5)
    ///     .Select(x => x * 2); // Result: Success(10)
    /// </example>
    public static Outcome<TResult, TFailure> Select<TSuccess, TFailure, TResult>(
        this Outcome<TSuccess, TFailure> outcome,
        Func<TSuccess, TResult> selector)
        where TSuccess : notnull
        where TFailure : notnull
        where TResult : notnull
    {
        return outcome.Match(
            success => Outcome<TResult, TFailure>.Success(selector(success)),
            Outcome<TResult, TFailure>.Failure);
    }

    /// <summary>
    /// Chains two Outcomes together by applying a function that returns a new Outcome
    /// to the success value of the first Outcome.
    /// </summary>
    /// <typeparam name="TSuccess">The original success type.</typeparam>
    /// <typeparam name="TFailure">The failure type.</typeparam>
    /// <typeparam name="TResult">The success type of the resulting Outcome.</typeparam>
    /// <param name="source">The source Outcome.</param>
    /// <param name="selector">The function that returns a new Outcome.</param>
    /// <returns>The resulting Outcome from the selector or the original error.</returns>
    /// <example>
    /// var result = Outcome&lt;int, string&gt;.Success(5)
    ///     .SelectMany(x => TryDivide(10, x)); // Chains operations
    /// </example>
    public static Outcome<TResult, TFailure> SelectMany<TSuccess, TFailure, TResult>(
        this Outcome<TSuccess, TFailure> source,
        Func<TSuccess, Outcome<TResult, TFailure>> selector)
        where TSuccess : notnull
        where TFailure : notnull
        where TResult : notnull
    {
        return source.Match(
            selector,
            Outcome<TResult, TFailure>.Failure);
    }

    /// <summary>
    /// Enables LINQ query syntax for Outcomes by chaining operations and projecting results.
    /// </summary>
    /// <typeparam name="TSuccess">The original success type.</typeparam>
    /// <typeparam name="TFailure">The failure type.</typeparam>
    /// <typeparam name="TIntermediate">The intermediate success type.</typeparam>
    /// <typeparam name="TResult">The final success type.</typeparam>
    /// <param name="source">The source Outcome.</param>
    /// <param name="intermediate">The function that returns an intermediate Outcome.</param>
    /// <param name="selector">The function that combines source and intermediate values.</param>
    /// <returns>A new Outcome with the combined result or the first error encountered.</returns>
    /// <example>
    /// // LINQ query syntax:
    /// var result = from x in Outcome&lt;int, string&gt;.Success(5)
    ///              from y in Outcome&lt;int, string&gt;.Success(10)
    ///              select x + y; // Result: Success(15)
    /// </example>
    public static Outcome<TResult, TFailure> SelectMany<TSuccess, TFailure, TIntermediate, TResult>(
        this Outcome<TSuccess, TFailure> source,
        Func<TSuccess, Outcome<TIntermediate, TFailure>> intermediate,
        Func<TSuccess, TIntermediate, TResult> selector)
        where TSuccess : notnull
        where TFailure : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        return source.Match(
            x =>
            {
                var elem = intermediate(x);
                return elem.Match(
                    y => Outcome<TResult, TFailure>.Success(selector(x, y)),
                    Outcome<TResult, TFailure>.Failure);
            },
            Outcome<TResult, TFailure>.Failure);
    }

    /// <summary>
    /// Transforms the success value of an awaited Outcome using the provided selector function.
    /// </summary>
    /// <typeparam name="TSuccess">The original success type.</typeparam>
    /// <typeparam name="TFailure">The failure type.</typeparam>
    /// <typeparam name="TResult">The result type after transformation.</typeparam>
    /// <param name="outcomeTask">The task returning a source Outcome.</param>
    /// <param name="selector">The function to transform the success value.</param>
    /// <returns>A task that returns a new Outcome with the transformed value or the original error.</returns>
    /// <example>
    /// var result = await GetUserAsync(userId)
    ///     .Select(user => user.Name); // Maps User to string
    /// </example>
    public static async Task<Outcome<TResult, TFailure>> Select<TSuccess, TFailure, TResult>(
        this Task<Outcome<TSuccess, TFailure>> outcomeTask,
        Func<TSuccess, TResult> selector)
        where TSuccess : notnull
        where TFailure : notnull
        where TResult : notnull
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return outcome.Select(selector);
    }

    /// <summary>
    /// Chains an awaited Outcome with an asynchronous operation that returns another Outcome.
    /// </summary>
    /// <typeparam name="TSuccess">The original success type.</typeparam>
    /// <typeparam name="TFailure">The failure type.</typeparam>
    /// <typeparam name="TResult">The success type of the resulting Outcome.</typeparam>
    /// <param name="outcomeTask">The task returning a source Outcome.</param>
    /// <param name="binder">The async function that returns a new Outcome.</param>
    /// <returns>A task that returns the resulting Outcome from the binder or the original error.</returns>
    /// <example>
    /// var result = await GetUserAsync(userId)
    ///     .SelectMany(user => SaveUserToDbAsync(user)); // Chains async operations
    /// </example>
    public static async Task<Outcome<TResult, TFailure>> SelectMany<TSuccess, TFailure, TResult>(
        this Task<Outcome<TSuccess, TFailure>> outcomeTask,
        Func<TSuccess, Task<Outcome<TResult, TFailure>>> binder)
        where TSuccess : notnull
        where TFailure : notnull
        where TResult : notnull
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return await outcome.Match(
            binder,
            error => Task.FromResult(Outcome<TResult, TFailure>.Failure(error))).ConfigureAwait(false);
    }

    /// <summary>
    /// Enables LINQ query syntax for awaited Outcomes by chaining async operations and projecting results.
    /// </summary>
    /// <typeparam name="TSuccess">The original success type.</typeparam>
    /// <typeparam name="TFailure">The failure type.</typeparam>
    /// <typeparam name="TIntermediate">The intermediate success type.</typeparam>
    /// <typeparam name="TResult">The final success type.</typeparam>
    /// <param name="outcomeTask">The task returning a source Outcome.</param>
    /// <param name="binder">The async function that returns an intermediate Outcome.</param>
    /// <param name="projector">The function that combines source and intermediate values.</param>
    /// <returns>A task that returns a new Outcome with the combined result or the first error encountered.</returns>
    /// <example>
    /// // Async LINQ query syntax:
    /// var result = await (from user in GetUserAsync(userId)
    ///                     from account in GetAccountAsync(user.AccountId)
    ///                     select new { user.Name, account.Balance });
    /// </example>
    public static async Task<Outcome<TResult, TFailure>> SelectMany<TSuccess, TFailure, TIntermediate, TResult>(
        this Task<Outcome<TSuccess, TFailure>> outcomeTask,
        Func<TSuccess, Task<Outcome<TIntermediate, TFailure>>> binder,
        Func<TSuccess, TIntermediate, TResult> projector)
        where TSuccess : notnull
        where TFailure : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        var outcome = await outcomeTask.ConfigureAwait(false);
        return await outcome.Match(
            async x =>
            {
                var intermediateOutcome = await binder(x).ConfigureAwait(false);
                return intermediateOutcome.Match(
                    y => Outcome<TResult, TFailure>.Success(projector(x, y)),
                    Outcome<TResult, TFailure>.Failure);
            },
            error => Task.FromResult(Outcome<TResult, TFailure>.Failure(error))
        ).ConfigureAwait(false);
    }
}