namespace JustTip.Domain.Entities;

public sealed class Business
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;

    private Business() { } // Required by EF Core

    public Business(string name)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Business name is required.", nameof(name))
            : name.Trim();
    }
}
