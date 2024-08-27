using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OktaPKCE.Controllers;

namespace OktaPKCE.Pages;

public class IndexModel : PageModel
{
    public const string OktaDomain = "dev-34014358.okta.com";
    public const string ClientId  = "0oaj7z5ysk96iMp6U5d7";
    public const string RedirectUri  = "http://localhost:5500/authorization-code/callback";
    public const string AuthEndpoint  = $"{OktaDomain}/oauth2/v1/authorize";

    private static readonly char[] _chars = 
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    
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
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.Now.AddMinutes(30)
        };
        
        Response.Cookies.Append("code_verifier", loginResponse.CodeVerifier, options);
        
        return Redirect(loginResponse.OktaRedirect);
    }
    
    private LoginResponse Authenticate()
    {
        var (codeVerifier, codeChallenge) = GeneratePkceCodes();
        var state = GenerateRandomString();
        // var authUrl = $"https://{AuthEndpoint}?response_type=code&state={state}&scope=openid profile email&client_id={ClientId}&redirect_uri={Uri.EscapeDataString(RedirectUri)}&code_challenge={codeChallenge}&code_challenge_method=S256";

        var authUrl = $"https://{AuthEndpoint}" +
                      $"?response_type=code" +
                      $"&client_id={Uri.EscapeDataString(ClientId)}" +
                      $"&state={Uri.EscapeDataString(state)}" +
                      $"&scope={Uri.EscapeDataString("openid")}" +
                      $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                      $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                      $"&code_challenge_method=S256";
        
        return new LoginResponse
        {
            CodeVerifier = codeVerifier,
            OktaRedirect = authUrl
        };
    }
    
    public string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.ASCII.GetBytes(codeVerifier);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    public static string GenerateRandomString()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[43];
        rng.GetBytes(bytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    private string Base64UrlEncode(string input)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(input);
        var base64 = Convert.ToBase64String(plainTextBytes);
        
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
    
    private (string CodeVerifier, string CodeChallenge) GeneratePkceCodes()
    {
        var codeVerifier = GenerateRandomString();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        
        Console.WriteLine($"Code Verifier: {codeVerifier}");
        Console.WriteLine($"Code Challenge: {codeChallenge}");
        
        return (codeVerifier, codeChallenge);
    }
}