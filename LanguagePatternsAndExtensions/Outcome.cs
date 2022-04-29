using System;
using System.Collections.Generic;

namespace LanguagePatternsAndExtensions
{
    public struct Outcome<TValue> : IEquatable<Outcome<TValue>>
    {
        public bool Equals(Outcome<TValue> other)
        {
            return EqualityComparer<TValue>.Default.Equals(Value, other.Value)
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
                var hashCode = EqualityComparer<TValue>.Default.GetHashCode(Value);
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

        public Outcome(TValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Value = value;
            ErrorMessage = "";
            Succeeded = true;
        }

        public Outcome(string errorMessage = "")
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentNullException(nameof(errorMessage), "error message string may not be empty");
            ErrorMessage = errorMessage;
            Succeeded = false;
            Value = default(TValue);
        }

        public TResult Traverse<TResult>(Func<TValue, TResult> success, Func<string, TResult> error)
        {
            if (success == null) throw new ArgumentNullException(nameof(success));
            if (error == null) throw new ArgumentNullException(nameof(error));

            if (Succeeded) return success(Value);
            return error(ErrorMessage);
        }

        public Unit Traverse(Action<TValue> success, Action<string> error)
        {
            if (success == null) throw new ArgumentNullException(nameof(success));
            if (error == null) throw new ArgumentNullException(nameof(error));

            if (Succeeded) success(Value);
            else error(ErrorMessage);

            return Unit.Default;
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
            if (Succeeded)
                return transform(Value);
            throw new OutcomeWasNotSuccessException();
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
            throw new OutcomeWasNotFailureException();
        }

        private readonly TValue Value;
        public string ErrorMessage { get; }
        public bool Succeeded { get; }
    }
}