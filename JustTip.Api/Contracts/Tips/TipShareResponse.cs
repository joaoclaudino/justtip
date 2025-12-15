namespace JustTip.Api.Contracts.Tips;

public sealed record TipShareResponse(Guid EmployeeId, decimal HoursWorked, decimal TipAmount);
