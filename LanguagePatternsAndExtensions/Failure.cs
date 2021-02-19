using System;

namespace LanguagePatternsAndExtensions
{
    public static class Failure
    {
        public static Outcome<T> Nok<T>(string errorMessage)
        {
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
            return new Outcome<T>(errorMessage);
        }
    }
}