using JustTip.Application.Tips;

namespace JustTip.UnitTests.Tips;

public sealed class TipDistributionServiceTests
{
    [Fact]
    public void Distribute_ProportionalSplit_ReturnsExpectedShares()
    {
        var svc = new TipDistributionService();
        var e1 = Guid.NewGuid();
        var e2 = Guid.NewGuid();

        var result = svc.Distribute(120m, new List<(Guid, decimal)>
        {
            (e1, 8m),
            (e2, 4m)
        });

        Assert.Equal(2, result.Count);

        var s1 = result.Single(x => x.EmployeeId == e1);
        var s2 = result.Single(x => x.EmployeeId == e2);

        Assert.Equal(80m, s1.TipAmount);
        Assert.Equal(40m, s2.TipAmount);
        Assert.Equal(120m, result.Sum(x => x.TipAmount));
    }

    [Fact]
    public void Distribute_AllZeroHours_Throws()
    {
        var svc = new TipDistributionService();
        var e1 = Guid.NewGuid();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            svc.Distribute(10m, new List<(Guid, decimal)> { (e1, 0m) })
        );

        Assert.Contains("0 total hours", ex.Message);
    }
}
