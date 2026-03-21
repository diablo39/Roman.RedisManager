using System.Runtime.CompilerServices;

namespace Roman.RedisManager.Tests
{
    public static class TestEnvironmentBootstrap
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }
    }
}
