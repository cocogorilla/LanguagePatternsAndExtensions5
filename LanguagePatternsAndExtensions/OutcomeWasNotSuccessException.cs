using System;

namespace LanguagePatternsAndExtensions
{
    public class OutcomeWasNotSuccessException : Exception
    {
        public OutcomeWasNotSuccessException() : base("using GetValue on Outcome type was improperly interpreted as a success when in error state")
        {
            
        }
    }
}