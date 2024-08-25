using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OktaPKCE.Controllers;

namespace OktaPKCE.Pages;

public class IndexModel : PageModel
{
    public const string OktaDomain = "dev-34014358.okta.com";
    public const string clientId  = "0oaj7z5sjtNh5pXfa5d7";
    public const string redirectUri  = "http://localhost:5500/authorization-code/callback";
    public const string authzEndpoint  = $"{OktaDomain}/oauth2/v1/authorize";

    public IActionResult OnGet()
    {
        if (Request.Cookies.ContainsKey("BearerToken"))
        {
            return Redirect("privacy");
        }

        var loginResponse = Authenticate();
        
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddMinutes(30)
        };
        
        Response.Cookies.Append("code_verifier", loginResponse.CodeVerifier, options);
        
        return Redirect(loginResponse.OktaRedirect);
    }
    
    private LoginResponse Authenticate()
    {
        var (codeVerifier, codeChallenge) = GeneratePkceCodes();
        var state = Base64UrlEncode(Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)));
        var authUrl = $"https://{authzEndpoint}?response_type=code&state={state}&scope=openid profile email&client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&code_challenge={codeChallenge}&code_challenge_method=S256";

        return new LoginResponse
        {
            CodeVerifier = codeVerifier,
            OktaRedirect = authUrl
        };
    }
    
    private string Sha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Base64UrlEncode(Convert.ToBase64String(hash));
    }
    
    private string Base64UrlEncode(string input)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(input);
        var base64 = Convert.ToBase64String(plainTextBytes);
        
        return base64.Replace("+", "-").Replace("/", "_").Replace("=", string.Empty);
    }
    
    private (string CodeVerifier, string CodeChallenge) GeneratePkceCodes()
    {
        var codeVerifier = Base64UrlEncode(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        var codeChallenge = Sha256(codeVerifier);
        
        return (codeVerifier, codeChallenge);
    }
}