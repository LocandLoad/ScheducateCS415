using System.Net;
using System.Text.RegularExpressions;

public static class AntiforgeryHelper
{
    public static async Task<HttpResponseMessage> PostAsync(
        HttpClient client,
        string formUrl,
        string postUrl,
        Dictionary<string, string> formValues)
    {
        // Step 1: GET the page containing the form
        using var getResponse = await client.GetAsync(formUrl);
        getResponse.EnsureSuccessStatusCode();

        var html = await getResponse.Content.ReadAsStringAsync();

        // Step 2: Find the hidden antiforgery input
        var inputMatch = Regex.Match(
            html,
            @"<input\b(?=[^>]*\bname\s*=\s*[""']__RequestVerificationToken[""'])[^>]*>",
            RegexOptions.IgnoreCase
        );

        if (!inputMatch.Success)
            throw new Exception("Antiforgery token not found in form.");

        // Step 3: Extract its value
        var valueMatch = Regex.Match(
            inputMatch.Value,
            @"\bvalue\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase
        );

        if (!valueMatch.Success)
            throw new Exception("Antiforgery token value not found.");

        var token = WebUtility.HtmlDecode(valueMatch.Groups[1].Value);

        // Step 4: Add token to POST form data
        formValues["__RequestVerificationToken"] = token;

        // Step 5: Submit POST request
        // The same HttpClient preserves the antiforgery cookie.
        return await client.PostAsync(
            postUrl,
            new FormUrlEncodedContent(formValues)
        );
    }
}