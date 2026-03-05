using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Roman.RedisManager.Tests.Web.Helpers
{
    internal static class ProblemDetailsAssertions
    {
        public static void ShouldBeValidProblemDetails(this Microsoft.AspNetCore.Mvc.ProblemDetails details)
        {
            details.ShouldNotBeNull();
            details.Type.ShouldNotBeNullOrWhiteSpace();
            details.Title.ShouldNotBeNullOrWhiteSpace();
            details.Status.ShouldNotBeNull();
            details.Status.Value.ShouldBeGreaterThan(0);
            // instance may be null depending on implementation
            details.Extensions.ShouldContainKey("traceId");
        }
    }
}
