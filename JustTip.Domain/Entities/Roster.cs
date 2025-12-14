namespace JustTip.Domain.Entities;

public sealed class Roster
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BusinessId { get; private set; }
    public DateOnly Date { get; private set; }

    private Roster() { } // EF

    public Roster(Guid businessId, DateOnly date)
    {
        BusinessId = businessId == Guid.Empty
            ? throw new ArgumentException("BusinessId is required.", nameof(businessId))
            : businessId;

        Date = date;
    }
}
