using Microsoft.AspNetCore.Mvc;

namespace JustTip.Api.Common;

public static class Validation
{
    public static IResult Problem400(string title, string detail)
        => Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status400BadRequest);

    public static IResult Problem404(string title, string detail)
        => Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);

    public static IResult Problem409(string title, string detail)
        => Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status409Conflict);

    public static (bool IsValid, string? Error) RequiredString(string? value, int maxLen, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (false, $"{fieldName} is required.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLen)
            return (false, $"{fieldName} must be at most {maxLen} characters.");

        return (true, null);
    }
}
