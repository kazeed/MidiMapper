using Xunit;

namespace MidiMapper.Tests;

public sealed class GeneralMidiToAd2MapTests
{
    private readonly GeneralMidiToAd2Map _sut = new();

    [Theory]
    [InlineData(35, 36)]
    [InlineData(36, 36)]
    [InlineData(37, 42)]
    [InlineData(38, 38)]
    [InlineData(39, 42)]
    [InlineData(40, 37)]
    [InlineData(41, 65)]
    [InlineData(42, 49)]
    [InlineData(43, 65)]
    [InlineData(44, 48)]
    [InlineData(45, 67)]
    [InlineData(46, 54)]
    [InlineData(47, 69)]
    [InlineData(48, 71)]
    [InlineData(49, 77)]
    [InlineData(50, 71)]
    [InlineData(51, 60)]
    [InlineData(52, 79)]
    [InlineData(53, 61)]
    [InlineData(54, 47)]
    [InlineData(55, 81)]
    [InlineData(56, 73)]
    [InlineData(57, 79)]
    [InlineData(59, 84)]
    [InlineData(75, 74)]
    public void TryMap_KnownNote_ReturnsExpectedTarget(byte source, byte expectedTarget)
    {
        var success = _sut.TryMap(source, out var target);

        Assert.True(success);
        Assert.Equal(expectedTarget, target);
    }

    [Theory]
    [InlineData(58)]
    [InlineData(80)]
    public void TryMap_UnknownNote_ReturnsFalse(byte source)
    {
        var success = _sut.TryMap(source, out _);

        Assert.False(success);
    }
}
