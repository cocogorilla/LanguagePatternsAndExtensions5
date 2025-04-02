using System;

namespace LanguagePatternsAndExtensions.Tests
{
    public record CustomError(string ErrorMessage, Guid TrackingId);
}
