using Roman.RedisManager.Web.Authorization;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class AuthorizationPoliciesTests
    {
        [Fact]
        public void ReaderPolicy_HasExpectedValue()
        {
            // Arrange
            var expected = "Reader";

            // Act
            var actual = AuthorizationPolicies.Reader;

            // Assert
            actual.ShouldBe(expected);
        }

        [Fact]
        public void EditorPolicy_HasExpectedValue()
        {
            // Arrange
            var expected = "Editor";

            // Act
            var actual = AuthorizationPolicies.Editor;

            // Assert
            actual.ShouldBe(expected);
        }

        [Fact]
        public void ReaderAndEditorPolicies_AreDifferent()
        {
            // Arrange & Act
            var reader = AuthorizationPolicies.Reader;
            var editor = AuthorizationPolicies.Editor;

            // Assert
            reader.ShouldNotBe(editor);
        }
    }
}
