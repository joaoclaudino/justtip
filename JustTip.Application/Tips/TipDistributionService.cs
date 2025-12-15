namespace JustTip.Application.Tips;

public sealed class TipDistributionService
{
    /// <summary>
    /// Distributes tips proportionally to worked hours.
    /// Rounds to 2 decimals and assigns any remainder to the last participant to ensure the total matches exactly.
    /// </summary>
    public IReadOnlyList<TipShare> Distribute(decimal totalTips, IReadOnlyList<(Guid EmployeeId, decimal HoursWorked)> hours)
    {
        if (totalTips < 0) throw new ArgumentException("Total tips must be >= 0.", nameof(totalTips));
        if (hours is null) throw new ArgumentNullException(nameof(hours));
        if (hours.Count == 0) throw new InvalidOperationException("Roster has no entries.");

        if (hours.Any(x => x.HoursWorked < 0))
            throw new InvalidOperationException("HoursWorked must be >= 0 for all employees.");

        var positive = hours.Where(x => x.HoursWorked > 0).ToList();
        if (positive.Count == 0)
            throw new InvalidOperationException("Roster has 0 total hours. Cannot distribute tips.");

        var totalHours = positive.Sum(x => x.HoursWorked);

        // Work in cents-like precision (2 decimals) but keep decimal to avoid floating errors.
        var shares = new List<TipShare>(positive.Count);
        decimal allocated = 0;

        for (int i = 0; i < positive.Count; i++)
        {
            var (employeeId, h) = positive[i];

            decimal amount;
            if (i == positive.Count - 1)
            {
                // Assign remainder to last to ensure exact total
                amount = Round2(totalTips - allocated);
            }
            else
            {
                var raw = totalTips * (h / totalHours);
                amount = Round2(raw);
                allocated += amount;
            }

            shares.Add(new TipShare(employeeId, h, amount));
        }

        // Just in case rounding created a tiny negative remainder due to extreme values
        // (shouldn't happen with decimal, but we keep it safe).
        if (shares.Any(s => s.TipAmount < 0))
            throw new InvalidOperationException("Invalid distribution result.");

        return shares;
    }

    private static decimal Round2(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
