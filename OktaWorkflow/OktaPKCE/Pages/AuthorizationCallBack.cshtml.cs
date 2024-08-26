using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace OktaPKCE.Pages;

public class AuthorizationCallBack : PageModel
{
    public const string OktaDomain = "dev-34014358.okta.com";
    public const string TokenEndpoint = $"https://{OktaDomain}/oauth2/v1/token";

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
        var clientId = "0oaj7z5sjtNh5pXfa5d7";
        var secretKey = "KlbrTK30dGZvWenrVL3p6cEFSCPVjtHmccKPnelJsdGMxpaQHwM3DOnrTyhpwxec";
        var redirectUri = "http://localhost:5500/authorization-code/callback";
        var tokenEndpoint = "https://dev-34014358.okta.com/oauth2/v1/token";

        var data = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", clientId },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier }
        };

        var content = new FormUrlEncodedContent(data);
        byte[] encodedSecrets = Encoding.UTF8.GetBytes($"{clientId}:{secretKey}");
        string secrets = Convert.ToBase64String(encodedSecrets);
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("authorization", $"Basic {secrets}");
        var response = await httpClient.PostAsync(TokenEndpoint, content);
        
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

        return result;
    }
}