using System;

namespace LanguagePatternsAndExtensions;

public static class Success
{
    public static Outcome<T> Of<T>(T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return Outcome<T>.Success(value);
    }

    public static Outcome<Unit> Ok()
    {
        return Outcome<Unit>.Success(Unit.Default);
    }

    /// <summary>
    /// Fully typed success helper
    /// </summary>
    /// <typeparam name="TSuccess">type of successful value</typeparam>
    /// <typeparam name="TError">type of failure value</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Outcome<TSuccess, TError> Of<TSuccess, TError>(TSuccess value) 
        where TSuccess : notnull 
        where TError : notnull
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return Outcome<TSuccess, TError>.Success(value);
    }

    /// <summary>
    /// Ok helper for type Unit, TError
    /// </summary>
    /// <typeparam name="TError">Carry forward the error type, Unit for success path assumed</typeparam>
    /// <returns></returns>
    public static Outcome<Unit, TError> Ok<TError>() where TError : notnull
    {
        return Outcome<Unit, TError>.Success(Unit.Default);
    }
}