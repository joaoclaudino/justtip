using System.Net.Http.Json;
using System.Text.Json;

var argsList = args?.ToList() ?? new List<string>();

if (argsList.Count == 0 || argsList.Contains("--help") || argsList.Contains("-h"))
{
    PrintHelp();
    return 0;
}

// Global option: --base-url
var baseUrl = GetOption(argsList, "--base-url") ?? "https://localhost:7035";
argsList = RemoveOption(argsList, "--base-url");

using var http = CreateHttp(baseUrl);

var command = argsList[0].ToLowerInvariant();
var sub = argsList.Count > 1 ? argsList[1].ToLowerInvariant() : "";

try
{
    switch (command)
    {
        case "businesses":
            return await HandleBusinesses(http, argsList.Skip(1).ToList());

        case "employees":
            return await HandleEmployees(http, argsList.Skip(1).ToList());

        case "rosters":
            return await HandleRosters(http, argsList.Skip(1).ToList());

        case "tips":
            return await HandleTips(http, argsList.Skip(1).ToList());

        default:
            Console.WriteLine($"Unknown command: {command}");
            PrintHelp();
            return 1;
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine("HTTP error:");
    Console.WriteLine(ex.Message);
    return 2;
}
catch (Exception ex)
{
    Console.WriteLine("Error:");
    Console.WriteLine(ex.Message);
    return 3;
}

static async Task<int> HandleBusinesses(HttpClient http, List<string> args)
{
    if (args.Count == 0 || args[0] is "--help" or "-h")
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  businesses list");
        Console.WriteLine("  businesses create --name \"Cafe A\"");
        return 0;
    }

    var action = args[0].ToLowerInvariant();

    if (action == "list")
    {
        var data = await http.GetFromJsonAsync<List<BusinessDto>>("/businesses");
        PrintJson(data);
        return 0;
    }

    if (action == "create")
    {
        var name = GetOption(args, "--name");
        if (string.IsNullOrWhiteSpace(name))
            return Fail("Missing required option: --name");

        var resp = await http.PostAsJsonAsync("/businesses", new { name });
        await PrintResponse(resp);
        return resp.IsSuccessStatusCode ? 0 : 1;
    }

    return Fail($"Unknown businesses action: {action}");
}

static async Task<int> HandleEmployees(HttpClient http, List<string> args)
{
    if (args.Count == 0 || args[0] is "--help" or "-h")
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  employees list --business-id <guid>");
        Console.WriteLine("  employees add  --business-id <guid> --name \"John\"");
        return 0;
    }

    var action = args[0].ToLowerInvariant();

    if (action == "list")
    {
        var businessId = GetGuidOption(args, "--business-id");
        if (businessId == Guid.Empty)
            return Fail("Missing/invalid required option: --business-id <guid>");

        var data = await http.GetFromJsonAsync<List<EmployeeDto>>($"/businesses/{businessId}/employees");
        PrintJson(data);
        return 0;
    }

    if (action == "add")
    {
        var businessId = GetGuidOption(args, "--business-id");
        if (businessId == Guid.Empty)
            return Fail("Missing/invalid required option: --business-id <guid>");

        var name = GetOption(args, "--name");
        if (string.IsNullOrWhiteSpace(name))
            return Fail("Missing required option: --name");

        var resp = await http.PostAsJsonAsync($"/businesses/{businessId}/employees", new { name });
        await PrintResponse(resp);
        return resp.IsSuccessStatusCode ? 0 : 1;
    }

    return Fail($"Unknown employees action: {action}");
}

static async Task<int> HandleRosters(HttpClient http, List<string> args)
{
    if (args.Count == 0 || args[0] is "--help" or "-h")
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  rosters ensure       --business-id <guid> --date 2025-12-14");
        Console.WriteLine("  rosters upsert-entry --business-id <guid> --date 2025-12-14 --employee-id <guid> --hours 8");
        return 0;
    }

    var action = args[0].ToLowerInvariant();

    if (action == "ensure")
    {
        var businessId = GetGuidOption(args, "--business-id");
        if (businessId == Guid.Empty)
            return Fail("Missing/invalid required option: --business-id <guid>");

        var date = GetOption(args, "--date");
        if (string.IsNullOrWhiteSpace(date))
            return Fail("Missing required option: --date yyyy-MM-dd");

        var resp = await http.PostAsync($"/businesses/{businessId}/rosters/{date}", null);
        await PrintResponse(resp);
        return resp.IsSuccessStatusCode ? 0 : 1;
    }

    if (action == "upsert-entry")
    {
        var businessId = GetGuidOption(args, "--business-id");
        if (businessId == Guid.Empty)
            return Fail("Missing/invalid required option: --business-id <guid>");

        var date = GetOption(args, "--date");
        if (string.IsNullOrWhiteSpace(date))
            return Fail("Missing required option: --date yyyy-MM-dd");

        var employeeId = GetGuidOption(args, "--employee-id");
        if (employeeId == Guid.Empty)
            return Fail("Missing/invalid required option: --employee-id <guid>");

        var hoursStr = GetOption(args, "--hours");
        if (!decimal.TryParse(hoursStr, out var hours))
            return Fail("Missing/invalid required option: --hours <number>");

        var resp = await http.PutAsJsonAsync(
            $"/businesses/{businessId}/rosters/{date}/entries",
            new { employeeId, hoursWorked = hours });

        await PrintResponse(resp);
        return resp.IsSuccessStatusCode ? 0 : 1;
    }

    return Fail($"Unknown rosters action: {action}");
}

static async Task<int> HandleTips(HttpClient http, List<string> args)
{
    if (args.Count == 0 || args[0] is "--help" or "-h")
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  tips distribute --business-id <guid> --date 2025-12-14 --total 120");
        return 0;
    }

    var action = args[0].ToLowerInvariant();

    if (action == "distribute")
    {
        var businessId = GetGuidOption(args, "--business-id");
        if (businessId == Guid.Empty)
            return Fail("Missing/invalid required option: --business-id <guid>");

        var date = GetOption(args, "--date");
        if (string.IsNullOrWhiteSpace(date))
            return Fail("Missing required option: --date yyyy-MM-dd");

        var totalStr = GetOption(args, "--total");
        if (!decimal.TryParse(totalStr, out var total))
            return Fail("Missing/invalid required option: --total <number>");

        var resp = await http.PostAsJsonAsync(
            $"/businesses/{businessId}/tips/{date}/distribute",
            new { totalTips = total });

        await PrintResponse(resp);
        return resp.IsSuccessStatusCode ? 0 : 1;
    }

    return Fail($"Unknown tips action: {action}");
}

static HttpClient CreateHttp(string baseUrl)
{
    var handler = new HttpClientHandler
    {
        // Dev convenience: allow HTTPS self-signed cert for localhost
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    };

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(baseUrl.TrimEnd('/'))
    };
}

static async Task PrintResponse(HttpResponseMessage resp)
{
    var text = await resp.Content.ReadAsStringAsync();
    Console.WriteLine($"HTTP {(int)resp.StatusCode} {resp.StatusCode}");
    Console.WriteLine(text);
}

static void PrintJson<T>(T? obj)
{
    var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(json);
}

static string? GetOption(List<string> args, string name)
{
    var idx = args.FindIndex(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
    if (idx < 0) return null;
    if (idx + 1 >= args.Count) return null;
    return args[idx + 1];
}

static List<string> RemoveOption(List<string> args, string name)
{
    var result = new List<string>(args.Count);
    for (int i = 0; i < args.Count; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            i++; // skip value
            continue;
        }
        result.Add(args[i]);
    }
    return result;
}

static Guid GetGuidOption(List<string> args, string name)
{
    var v = GetOption(args, name);
    return Guid.TryParse(v, out var id) ? id : Guid.Empty;
}

static int Fail(string message)
{
    Console.WriteLine(message);
    Console.WriteLine();
    PrintHelp();
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("JustTip CLI");
    Console.WriteLine();
    Console.WriteLine("Global:");
    Console.WriteLine("  --base-url <url>     API base URL (default: https://localhost:7035)");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  businesses list");
    Console.WriteLine("  businesses create --name \"Cafe A\"");
    Console.WriteLine();
    Console.WriteLine("  employees list --business-id <guid>");
    Console.WriteLine("  employees add  --business-id <guid> --name \"John\"");
    Console.WriteLine();
    Console.WriteLine("  rosters ensure       --business-id <guid> --date 2025-12-14");
    Console.WriteLine("  rosters upsert-entry --business-id <guid> --date 2025-12-14 --employee-id <guid> --hours 8");
    Console.WriteLine();
    Console.WriteLine("  tips distribute --business-id <guid> --date 2025-12-14 --total 120");
}
sealed record BusinessDto(Guid Id, string Name);
sealed record EmployeeDto(Guid Id, Guid BusinessId, string Name);
