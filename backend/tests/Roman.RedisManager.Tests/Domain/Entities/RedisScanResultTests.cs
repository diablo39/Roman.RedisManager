using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Tests.Domain.Entities
{
    public class RedisScanResultTests
    {
        [Fact]
        public void HasMoreResults_WhenCursorIsZero_ReturnsFalse()
        {
            var result = new RedisScanResult<string>(cursor: 0, items: new[] { "a", "b" });

            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public void HasMoreResults_WhenCursorIsNonZero_ReturnsTrue()
        {
            var result = new RedisScanResult<string>(cursor: 42, items: new[] { "a" });

            result.HasMoreResults.ShouldBeTrue();
        }

        [Fact]
        public void Items_ReturnsSuppliedItems()
        {
            var items = new[] { "x", "y", "z" };

            var result = new RedisScanResult<string>(cursor: 0, items: items);

            result.Items.ShouldBe(items);
        }

        [Fact]
        public void Cursor_ReturnsSuppliedCursor()
        {
            var result = new RedisScanResult<int>(cursor: 999, items: Array.Empty<int>());

            result.Cursor.ShouldBe(999);
        }
    }
}
