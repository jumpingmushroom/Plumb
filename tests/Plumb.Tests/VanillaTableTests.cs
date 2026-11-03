using Plumb.Core.Model;
using Xunit;

public class VanillaTableTests
{
    [Fact]
    public void TrustedScanKeepsEachMaterialsOwnValueOnNameCollision()
    {
        var t = new VanillaTable();
        Assert.Equal(0.05f, t.Record("X", 0.05f, trusted: true));
        Assert.Equal(0.5f, t.Record("X", 0.5f, trusted: true));
    }

    [Fact]
    public void LaterCopyUsesFirstRecordedValueForItsName()
    {
        var t = new VanillaTable();
        t.Record("wood", 0.1f, trusted: true);
        Assert.Equal(0.1f, t.Record("wood", 0.01f, trusted: false));
        Assert.Equal(0.1f, t.Record("wood (Instance)", 0f, trusted: false));
        Assert.Equal(0.1f, t.Record("wood (Instance) (Instance)", 0f, trusted: false));
        Assert.Equal(1, t.Count);
    }

    [Fact]
    public void LaterMaterialWithNewNameRecordsItsValue()
    {
        var t = new VanillaTable();
        Assert.Equal(0.3f, t.Record("modded", 0.3f, trusted: false));
        Assert.Equal(0.3f, t.Record("modded (Instance)", 0.03f, trusted: false));
    }

    [Fact]
    public void BaseNameStripsAllInstanceSuffixes() =>
        Assert.Equal("stone", VanillaTable.BaseName("stone (Instance) (Instance)"));
}
