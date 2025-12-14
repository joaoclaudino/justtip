namespace JustTip.Api.Contracts.Rosters;

public sealed record UpsertRosterEntryRequest(Guid EmployeeId, decimal HoursWorked);
