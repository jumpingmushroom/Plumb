using Plumb.Core.Model;
using Xunit;

public class VanillaTableTests
{
    [Fact]
    public void FirstSeenWins()
    {
        var t = new VanillaTable();
        Assert.Equal(0.1f, t.Resolve("wood", 0.1f));
        Assert.Equal(0.1f, t.Resolve("wood", 0.05f));
    }

    [Fact]
    public void InstanceCopyUsesBase()
    {
        var t = new VanillaTable();
        t.Resolve("wood", 0.1f);
        Assert.Equal(0.1f, t.Resolve("wood (Instance)", 0f));
        Assert.Equal(0.1f, t.Resolve("wood (Instance) (Instance)", 0f));
        Assert.Equal(1, t.Count);
    }

    [Fact]
    public void BaseNameStripsAllInstanceSuffixes() =>
        Assert.Equal("stone", VanillaTable.BaseName("stone (Instance) (Instance)"));

    [Fact]
    public void DistinctNamesKeptApart()
    {
        var t = new VanillaTable();
        t.Resolve("wood", 0.1f);
        t.Resolve("stone", 0.08f);
        Assert.Equal(0.08f, t.Resolve("stone", 0f));
    }
}
