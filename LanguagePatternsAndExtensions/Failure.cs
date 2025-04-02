using System;

namespace LanguagePatternsAndExtensions;

public static class Failure
{
    public static Outcome<T> Nok<T>(string errorMessage)
    {
        if (errorMessage == null)
            throw new ArgumentNullException(nameof(errorMessage));
        return Outcome<T>.Failure(errorMessage);
    }

    public static Outcome<TSuccess, TFailure> Nok<TSuccess, TFailure>(TFailure error)
        where TSuccess : notnull
        where TFailure : notnull
    {
        if (error == null)
            throw new ArgumentNullException(nameof(error));
        return Outcome<TSuccess, TFailure>.Failure(error);
    }
}