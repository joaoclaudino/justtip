namespace JustTip.Application.Tips;

public sealed record TipShare(Guid EmployeeId, decimal HoursWorked, decimal TipAmount);
