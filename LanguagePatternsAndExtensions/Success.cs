using System;

namespace LanguagePatternsAndExtensions
{
    public static class Success
    {
        public static Outcome<T> Of<T>(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new Outcome<T>(value);
        }

        public static Outcome<Unit> Ok()
        {
            return new Outcome<Unit>(Unit.Default);
        }
    }
}