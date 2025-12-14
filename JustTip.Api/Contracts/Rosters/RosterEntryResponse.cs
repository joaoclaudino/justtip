namespace JustTip.Api.Contracts.Rosters;

public sealed record RosterEntryResponse(Guid Id, Guid RosterId, Guid EmployeeId, decimal HoursWorked);
