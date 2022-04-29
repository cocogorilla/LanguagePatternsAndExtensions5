using System;

namespace LanguagePatternsAndExtensions
{
    public class OutcomeWasNotFailureException : Exception
    {
        public OutcomeWasNotFailureException() : base("using GetError on Outcome type was improperly interpreted as a failure when in success state")
        {

        }
    }
}