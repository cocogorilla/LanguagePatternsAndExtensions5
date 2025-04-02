using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LanguagePatternsAndExtensions;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail
/// </summary>
/// <typeparam name="TValue">The type of value returned from the successful operation</typeparam>
public readonly struct Outcome<TValue> : IEquatable<Outcome<TValue>>
{
    public bool Equals(Outcome<TValue> other)
    {
        return EqualityComparer<TValue>.Default.Equals(_value, other._value)
               && string.Equals(ErrorMessage, other.ErrorMessage, StringComparison.InvariantCulture)
               && Succeeded == other.Succeeded;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Outcome<TValue> outcome && Equals(outcome);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = EqualityComparer<TValue>.Default.GetHashCode(_value);
            hashCode = (hashCode * 397) ^ (ErrorMessage != null ? StringComparer.InvariantCulture.GetHashCode(ErrorMessage) : 0);
            hashCode = (hashCode * 397) ^ Succeeded.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Outcome<TValue> left, Outcome<TValue> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Outcome<TValue> left, Outcome<TValue> right)
    {
        return !Equals(left, right);
    }

    private Outcome(TValue value, bool isSuccess)
    {
        if (isSuccess && value == null)
            throw new ArgumentNullException(nameof(value));
        _value = value;
        ErrorMessage = isSuccess ? "" : value as string ?? "";
        Succeeded = isSuccess;
    }

    public static Outcome<TValue> Success(TValue value)
    {
        return new Outcome<TValue>(value, true);
    }

    public static Outcome<TValue> Failure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        var failure = new Outcome<TValue>(default, false)
        {
            ErrorMessage = errorMessage
        };
        return failure;
    }

    [Obsolete("use match")]
    public TResult Traverse<TResult>(Func<TValue, TResult> success, Func<string, TResult> error)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (error == null) throw new ArgumentNullException(nameof(error));

        if (Succeeded) return success(_value);
        return error(ErrorMessage);
    }

    [Obsolete("use match")]
    public Unit Traverse(Action<TValue> success, Action<string> error)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (error == null) throw new ArgumentNullException(nameof(error));

        if (Succeeded) success(_value);
        else error(ErrorMessage);

        return Unit.Default;
    }

    [Obsolete("use match")]
    public async Task<TResult> TraverseAsync<TResult>(
        Func<TValue, Task<TResult>> success,
        Func<string, Task<TResult>> error)
    {
        if (Succeeded)
            return await success(_value).ConfigureAwait(false);
        else
            return await error(ErrorMessage).ConfigureAwait(false);
    }

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<string, TResult> failure)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (failure == null) throw new ArgumentNullException(nameof(failure));

        return Succeeded ? success(_value) : failure(ErrorMessage);
    }

    public Unit Match(Action<TValue> success, Action<string> failure)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (failure == null) throw new ArgumentNullException(nameof(failure));

        if (Succeeded) success(_value);
        else failure(ErrorMessage);

        return Unit.Default;
    }

    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> success,
        Func<string, Task<TResult>> failure)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (failure == null) throw new ArgumentNullException(nameof(failure));

        return Succeeded
            ? await success(_value).ConfigureAwait(false)
            : await failure(ErrorMessage).ConfigureAwait(false);
    }

    /// <summary>
    /// Unsafe, directly retrieve a value assuming it is not null and apply a func transform
    /// Example: var theValue = outcome.GetValue(x => x);
    /// theValue may be null
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    public TResult GetValue<TResult>(Func<TValue, TResult> transform)
    {
        if (transform == null) throw new ArgumentNullException(nameof(transform));

        if (Succeeded)
            return transform(_value);

        throw new InvalidOperationException($"Cannot access value on a failed outcome with error: {ErrorMessage}");
    }

    /// <summary>
    /// Unsafe, the outcome may not be in an error state
    /// Example: var theError = outcome.GetError();
    /// theError may be empty
    /// </summary>
    /// <returns></returns>
    public string GetError()
    {
        if (!Succeeded)
            return ErrorMessage;

        throw new InvalidOperationException("Cannot access error on a successful outcome");
    }

    public bool IsSuccess => Succeeded;
    public bool IsFailure => !Succeeded;

    private readonly TValue _value;
    public string ErrorMessage { get; init; }
    public bool Succeeded { get; }

    public override string ToString()
    {
        return Succeeded ? $"Success: {_value}" : $"Failure: {ErrorMessage}";
    }
}

/// <summary>
/// Represents the result of an operation that might succeed with a value or fail with an error.
/// This struct is designed to eliminate null values and provide clearer error handling.
/// </summary>
/// <typeparam name="TSuccess">The type of the successful result value.</typeparam>
/// <typeparam name="TFailure">The type of the error information when the operation fails.</typeparam>
public readonly struct Outcome<TSuccess, TFailure> : IEquatable<Outcome<TSuccess, TFailure>>
    where TSuccess : notnull
    where TFailure : notnull
{
    /// <summary>
    /// Compares this Outcome with another for equality.
    /// </summary>
    /// <param name="other">The Outcome to compare with.</param>
    /// <returns>True if both Outcomes have the same success state and equivalent values.</returns>
    public bool Equals(Outcome<TSuccess, TFailure> other)
    {
        return EqualityComparer<TSuccess>.Default.Equals(_value, other._value)
               && EqualityComparer<TFailure>.Default.Equals(_error, other._error)
               && Succeeded == other.Succeeded;
    }

    /// <summary>
    /// Compares this Outcome with another object for equality.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is an Outcome with the same success state and equivalent values.</returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Outcome<TSuccess, TFailure> outcome && Equals(outcome);
    }

    /// <summary>
    /// Generates a hash code for this Outcome.
    /// </summary>
    /// <returns>A hash code based on the success state and values.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = EqualityComparer<TSuccess>.Default.GetHashCode(_value);
            hashCode = (hashCode * 397) ^ EqualityComparer<TFailure>.Default.GetHashCode(_error);
            hashCode = (hashCode * 397) ^ Succeeded.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Equality operator for comparing two Outcomes.
    /// </summary>
    /// <param name="left">The first Outcome to compare.</param>
    /// <param name="right">The second Outcome to compare.</param>
    /// <returns>True if both Outcomes are equal.</returns>
    public static bool operator ==(Outcome<TSuccess, TFailure> left, Outcome<TSuccess, TFailure> right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator for comparing two Outcomes.
    /// </summary>
    /// <param name="left">The first Outcome to compare.</param>
    /// <param name="right">The second Outcome to compare.</param>
    /// <returns>True if the Outcomes are not equal.</returns>
    public static bool operator !=(Outcome<TSuccess, TFailure> left, Outcome<TSuccess, TFailure> right)
    {
        return !Equals(left, right);
    }

    // Private constructor
    private Outcome(TSuccess value, TFailure failure, bool isSuccess)
    {
        if (isSuccess && value == null)
            throw new ArgumentNullException(nameof(value));
        if (!isSuccess && failure == null)
            throw new ArgumentNullException(nameof(failure));
        _value = value;
        _error = failure;
        Succeeded = isSuccess;
    }

    /// <summary>
    /// Creates a successful Outcome with the specified value.
    /// </summary>
    /// <param name="value">The successful result value (cannot be null).</param>
    /// <returns>A new Outcome representing success.</returns>
    /// <example>
    /// var result = Outcome&lt;int, string&gt;.Success(42);
    /// </example>
    public static Outcome<TSuccess, TFailure> Success(TSuccess value)
    {
        return new Outcome<TSuccess, TFailure>(value, default, true);
    }

    /// <summary>
    /// Creates a failed Outcome with the specified error.
    /// </summary>
    /// <param name="error">Information about the error (cannot be null).</param>
    /// <returns>A new Outcome representing failure.</returns>
    /// <example>
    /// var result = Outcome&lt;int, Exception&gt;.Failure(new ArgumentException("Invalid input"));
    /// </example>
    public static Outcome<TSuccess, TFailure> Failure(TFailure error)
    {
        return new Outcome<TSuccess, TFailure>(default, error, false);
    }

    /// <summary>
    /// Processes the Outcome by applying one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after processing.</typeparam>
    /// <param name="success">Function to apply if this Outcome represents success.</param>
    /// <param name="failure">Function to apply if this Outcome represents failure.</param>
    /// <returns>The result of applying the appropriate function.</returns>
    /// <example>
    /// var displayText = result.Match(
    ///     success: value => $"The answer is {value}",
    ///     failure: error => $"Error occurred: {error}"
    /// );
    /// </example>
    public TResult Match<TResult>(
        Func<TSuccess, TResult> success,
        Func<TFailure, TResult> failure)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (failure == null) throw new ArgumentNullException(nameof(failure));

        return Succeeded ? success(_value!) : failure(_error!);
    }

    /// <summary>
    /// Executes one of two actions based on whether this Outcome represents success or failure.
    /// </summary>
    /// <param name="success">Action to execute if this Outcome represents success.</param>
    /// <param name="failure">Action to execute if this Outcome represents failure.</param>
    /// <returns>A Unit value (placeholder for void).</returns>
    /// <example>
    /// result.Match(
    ///     success: value => Console.WriteLine($"Success: {value}"),
    ///     failure: error => Console.WriteLine($"Error: {error}")
    /// );
    /// </example>
    public Unit Match(
        Action<TSuccess> success,
        Action<TFailure> failure)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (failure == null) throw new ArgumentNullException(nameof(failure));

        if (Succeeded) success(_value!);
        else failure(_error!);

        return Unit.Default;
    }

    /// <summary>
    /// Asynchronously processes the Outcome by applying one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after processing.</typeparam>
    /// <param name="success">Async function to apply if this Outcome represents success.</param>
    /// <param name="failure">Async function to apply if this Outcome represents failure.</param>
    /// <returns>A task that represents the result of applying the appropriate function.</returns>
    /// <example>
    /// var result = await outcomeValue.MatchAsync(
    ///     success: async value => await SaveToDatabase(value),
    ///     failure: async error => await LogError(error)
    /// );
    /// </example>
    public async Task<TResult> MatchAsync<TResult>(
        Func<TSuccess, Task<TResult>> success,
        Func<TFailure, Task<TResult>> failure)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));
        if (failure == null) throw new ArgumentNullException(nameof(failure));

        return Succeeded
            ? await success(_value!).ConfigureAwait(false)
            : await failure(_error!).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a value indicating whether this Outcome represents success.
    /// </summary>
    public bool IsSuccess => Succeeded;
    /// <summary>
    /// Gets a value indicating whether this Outcome represents failure.
    /// </summary>
    public bool IsFailure => !Succeeded;

    // Private fields and public properties
    private readonly TSuccess? _value;
    private readonly TFailure? _error;

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Returns a string representation of this Outcome, including its success state and value.
    /// </summary>
    /// <returns>A string representation of this Outcome.</returns>
    public override string ToString()
    {
        return Succeeded ? $"Success: {_value}" : $"Failure: {_error}";
    }
}