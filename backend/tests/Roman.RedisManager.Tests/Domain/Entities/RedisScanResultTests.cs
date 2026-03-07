using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Tests.Domain.Entities
{
    public class RedisScanResultTests
    {
        [Fact]
        public void HasMoreResults_WhenCursorIsZero_ReturnsFalse()
        {

            // Arrange
            var result = new RedisScanResult<string>(cursor: 0, items: new[] { "a", "b" });

            // Act

            // Assert
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public void HasMoreResults_WhenCursorIsNonZero_ReturnsTrue()
        {

            // Arrange
            var result = new RedisScanResult<string>(cursor: 42, items: new[] { "a" });

            // Act

            // Assert
            result.HasMoreResults.ShouldBeTrue();
        }

        [Fact]
        public void Items_ReturnsSuppliedItems()
        {

            // Arrange
            var items = new[] { "x", "y", "z" };

            var result = new RedisScanResult<string>(cursor: 0, items: items);

            // Act

            // Assert
            result.Items.ShouldBe(items);
        }

        [Fact]
        public void Cursor_ReturnsSuppliedCursor()
        {

            // Arrange
            var result = new RedisScanResult<int>(cursor: 999, items: Array.Empty<int>());

            // Act

            // Assert
            result.Cursor.ShouldBe(999);
        }
    }
}
