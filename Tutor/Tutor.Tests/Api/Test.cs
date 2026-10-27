using FluentAssertions;
using NUnit.Framework;

namespace Tutor.Tests.Api;

public class Test
{
    [Test]
    public async Task Should_Be_Success()
    {
        var result = true;

        result.Should().Be(true);
    }
}