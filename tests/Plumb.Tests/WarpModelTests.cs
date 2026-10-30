using Plumb.Core.Model;
using Xunit;

public class WarpModelTests
{
    [Fact] public void Straight() => Assert.Equal(0f, WarpModel.Target(0.1f, 0f, false));
    [Fact] public void Vanilla() => Assert.Equal(0.1f, WarpModel.Target(0.1f, 1f, false), 6);
    [Fact] public void Triple() => Assert.Equal(0.3f, WarpModel.Target(0.1f, 3f, false), 6);
    [Fact] public void ClampsHigh() => Assert.Equal(0.3f, WarpModel.Target(0.1f, 9f, false), 6);
    [Fact] public void ClampsLow() => Assert.Equal(0f, WarpModel.Target(0.1f, -1f, false));
    [Fact] public void CompareShowsVanilla() => Assert.Equal(0.1f, WarpModel.Target(0.1f, 0f, true));

    [Theory]
    [InlineData("0", 0f)] [InlineData("0.5", 0.5f)] [InlineData("3", 3f)] [InlineData(" 1 ", 1f)]
    public void ParsesValid(string s, float v)
    {
        Assert.True(WarpModel.TryParseStrength(s, out var r));
        Assert.Equal(v, r);
    }

    [Theory]
    [InlineData("0,5")] [InlineData("-1")] [InlineData("3.01")] [InlineData("NaN")]
    [InlineData("Infinity")] [InlineData("abc")] [InlineData("")] [InlineData(null)]
    public void RejectsInvalid(string s) => Assert.False(WarpModel.TryParseStrength(s, out _));
}
