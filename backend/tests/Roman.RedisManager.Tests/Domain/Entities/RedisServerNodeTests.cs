using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Tests.Domain.Entities
{
    public class RedisServerNodeTests
    {
        [Fact]
        public void Constructor_ValidArguments_SetsProperties()
        {
            var node = new RedisServerNode("redis-host", 6379, "MASTER");

            node.Host.ShouldBe("redis-host");
            node.Port.ShouldBe(6379);
            node.Role.ShouldBe("master"); // normalised to lower
        }

        [Fact]
        public void Constructor_NullHost_ThrowsArgumentException()
        {
            var ex = Should.Throw<ArgumentException>(() => new RedisServerNode(null!, 6379, "master"));
            ex.ParamName.ShouldBe("host");
        }

        [Fact]
        public void Constructor_WhitespaceHost_ThrowsArgumentException()
        {
            var ex = Should.Throw<ArgumentException>(() => new RedisServerNode("   ", 6379, "master"));
            ex.ParamName.ShouldBe("host");
        }

        [Fact]
        public void Constructor_PortZero_ThrowsArgumentOutOfRangeException()
        {
            var ex = Should.Throw<ArgumentOutOfRangeException>(() => new RedisServerNode("localhost", 0, "master"));
            ex.ParamName.ShouldBe("port");
        }

        [Fact]
        public void Constructor_NegativePort_ThrowsArgumentOutOfRangeException()
        {
            var ex = Should.Throw<ArgumentOutOfRangeException>(() => new RedisServerNode("localhost", -1, "master"));
            ex.ParamName.ShouldBe("port");
        }

        [Fact]
        public void Constructor_NullRole_ThrowsArgumentException()
        {
            var ex = Should.Throw<ArgumentException>(() => new RedisServerNode("localhost", 6379, null!));
            ex.ParamName.ShouldBe("role");
        }

        [Fact]
        public void Constructor_WhitespaceRole_ThrowsArgumentException()
        {
            var ex = Should.Throw<ArgumentException>(() => new RedisServerNode("localhost", 6379, "  "));
            ex.ParamName.ShouldBe("role");
        }

        [Fact]
        public void Constructor_RoleNormalisedToLowercase()
        {
            var node = new RedisServerNode("localhost", 6380, "SLAVE");

            node.Role.ShouldBe("slave");
        }
    }
}
