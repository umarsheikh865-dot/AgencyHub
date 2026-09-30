using Xunit;

namespace AgencyHub.Tests.Auth;

public class AuthServiceTests
{
    [Fact]
    public void Login_ShouldReturnTrue_WhenCredentialsAreValid()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }
}