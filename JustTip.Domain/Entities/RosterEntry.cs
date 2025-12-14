namespace JustTip.Domain.Entities;

public sealed class RosterEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid RosterId { get; private set; }
    public Guid EmployeeId { get; private set; }

    public decimal HoursWorked { get; private set; }

    private RosterEntry() { } // EF

    public RosterEntry(Guid rosterId, Guid employeeId, decimal hoursWorked)
    {
        if (rosterId == Guid.Empty) throw new ArgumentException("RosterId is required.", nameof(rosterId));
        if (employeeId == Guid.Empty) throw new ArgumentException("EmployeeId is required.", nameof(employeeId));
        if (hoursWorked < 0) throw new ArgumentException("HoursWorked must be >= 0.", nameof(hoursWorked));

        RosterId = rosterId;
        EmployeeId = employeeId;
        HoursWorked = hoursWorked;
    }

    public void SetHours(decimal hoursWorked)
    {
        if (hoursWorked < 0) throw new ArgumentException("HoursWorked must be >= 0.", nameof(hoursWorked));
        HoursWorked = hoursWorked;
    }
}
