using System;

namespace LanguagePatternsAndExtensions;

public readonly struct Option<T>
{
    private readonly T _item;

    public Option(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _item = item;
        IsSome = true;
    }

    public Option(Unit none)
    {
        _item = default;
        IsSome = false;
    }

    public bool IsSome { get; }
    public bool IsNone => !IsSome;

    public TResult Match<TResult>(TResult nothing, Func<T, TResult> some)
    {
        if (nothing == null) throw new ArgumentNullException(nameof(nothing));
        if (some == null) throw new ArgumentNullException(nameof(some));

        return (IsSome)
            ? some(_item)
            : nothing;
    }

    public TResult Match<TResult>(Func<TResult> none, Func<T, TResult> some)
    {
        if (none == null) throw new ArgumentNullException(nameof(none));
        if (some == null) throw new ArgumentNullException(nameof(some));
        return (IsSome)
            ? some(_item)
            : none();
    }

    /// <summary>
    /// Unsafe, directly retrieve a value assuming it is not null and apply a func transform
    /// Example: var theValue = optional.GetValue(x => x);
    /// theValue may be null
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    public TResult GetValue<TResult>(Func<T, TResult> transform)
    {
        return transform(_item);
    }
    public static Option<T> Some(T source)
    {
        return source == null
            ? None()
            : new Option<T>(source);
    }

    public static Option<T> None()
    {
        return new Option<T>(Unit.Default);
    }

    public static bool operator true(Option<T> option) => option.IsSome;
    public static bool operator false(Option<T> option) => !option.IsSome;


    public void Deconstruct(out bool isSome, out T item)
    {
        isSome = IsSome;
        item = _item;
    }

    public static implicit operator Option<T>(T value) => value is null ? None() : new Option<T>(value);
}