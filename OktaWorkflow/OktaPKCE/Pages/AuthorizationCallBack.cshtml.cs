using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Okta.Auth.Sdk;
using Okta.Sdk.Abstractions.Configuration;

namespace OktaPKCE.Pages;

public class AuthorizationCallBack : PageModel
{
    public const string TokenEndpoint = "https://dev-34014358.okta.com/oauth2/v1/token";

    public void OnGet()
    {
    }

    public async Task<IActionResult> HandleRedirect(string codeVerifier)
    {
        var queryParams = Request.HttpContext.Request.Query;

        if (queryParams.TryGetValue("code", out StringValues codeValues))
        {
            var code = codeValues.ToString();
            var tokenResponse = await ExchangeCodeForTokenAsync(code, codeVerifier);

            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(30)
            };

            Response.Cookies.Append("Token", JsonConvert.SerializeObject(tokenResponse, Formatting.Indented), options);

            return Redirect("privacy");
        }

        return Content("No code parameter found in the query string.", "text/plain");
    }

    private async Task<Dictionary<string, object>> ExchangeCodeForTokenAsync(string code, string codeVerifier)
    {
        var clientId = "0oaj7z5ysk96iMp6U5d7";
        var redirectUri = "http://localhost:5500/authorization-code/callback";

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", clientId },
            { "redirect_uri", redirectUri },
            { "code", code },
            { "code_verifier", codeVerifier }
        };

        var content = new FormUrlEncodedContent(data);
        var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = content 
        };
        
        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

        return result;
    }
}