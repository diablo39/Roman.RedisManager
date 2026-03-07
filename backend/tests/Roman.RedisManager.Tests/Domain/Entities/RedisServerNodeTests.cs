using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Tests.Domain.Entities
{
    public class RedisServerNodeTests
    {
        [Fact]
        public void Constructor_ValidArguments_SetsProperties()
        {
            // Arrange
            var node = new RedisServerNode("redis-host", 6379, "MASTER");

            // Act

            // Assert
            node.Host.ShouldBe("redis-host");
            node.Port.ShouldBe(6379);
            node.Role.ShouldBe("master"); // normalised to lower
        }

        [Fact]
        public void Constructor_NullHost_ThrowsArgumentException()
        {
            // Arrange
            var action = () => new RedisServerNode(null!, 6379, "master");

            // Act
            var ex = Should.Throw<ArgumentException>(action);

            // Assert
            ex.ParamName.ShouldBe("host");
        }

        [Fact]
        public void Constructor_WhitespaceHost_ThrowsArgumentException()
        {
            // Arrange
            var action = () => new RedisServerNode("   ", 6379, "master");

            // Act
            var ex = Should.Throw<ArgumentException>(action);

            // Assert
            ex.ParamName.ShouldBe("host");
        }

        [Fact]
        public void Constructor_PortZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var action = () => new RedisServerNode("localhost", 0, "master");

            // Act
            var ex = Should.Throw<ArgumentOutOfRangeException>(action);

            // Assert
            ex.ParamName.ShouldBe("port");
        }

        [Fact]
        public void Constructor_NegativePort_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var action = () => new RedisServerNode("localhost", -1, "master");

            // Act
            var ex = Should.Throw<ArgumentOutOfRangeException>(action);

            // Assert
            ex.ParamName.ShouldBe("port");
        }

        [Fact]
        public void Constructor_NullRole_ThrowsArgumentException()
        {
            // Arrange
            var action = () => new RedisServerNode("localhost", 6379, null!);

            // Act
            var ex = Should.Throw<ArgumentException>(action);

            // Assert
            ex.ParamName.ShouldBe("role");
        }

        [Fact]
        public void Constructor_WhitespaceRole_ThrowsArgumentException()
        {
            // Arrange
            var action = () => new RedisServerNode("localhost", 6379, "  ");

            // Act
            var ex = Should.Throw<ArgumentException>(action);

            // Assert
            ex.ParamName.ShouldBe("role");
        }

        [Fact]
        public void Constructor_RoleNormalisedToLowercase()
        {
            // Arrange
            var node = new RedisServerNode("localhost", 6380, "SLAVE");

            // Act

            // Assert
            node.Role.ShouldBe("slave");
        }
    }
}
