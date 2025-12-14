namespace JustTip.Domain.Entities;

public sealed class Employee
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BusinessId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Employee() { } // Required by EF Core

    public Employee(Guid businessId, string name)
    {
        BusinessId = businessId == Guid.Empty
            ? throw new ArgumentException("BusinessId is required.", nameof(businessId))
            : businessId;

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Employee name is required.", nameof(name))
            : name.Trim();
    }
}
