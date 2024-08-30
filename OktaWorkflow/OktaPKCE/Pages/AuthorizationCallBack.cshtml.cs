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


/*
 * Token Example:
 * {
     "token_type": "Bearer",
     "expires_in": 3600,
     "access_token": "eyJraWQiOiJnQ1EtWDFxV1ZoYWFnUmV5T0xyMWQ2LVhLd1ZTNjRYX0FXQjBQX01mUkhFIiwidHlwIjoiYXBwbGljYXRpb25cL29rdGEtaW50ZXJuYWwtYXQrand0IiwiYWxnIjoiUlMyNTYifQ.eyJ2ZXIiOjEsImp0aSI6IkFULldzSUZHdHJJMUF2UnhzM0RDX3FWYmpzUnRhLWtXLVdUcXhQLUdDZDVxbzgiLCJpc3MiOiJodHRwczovL2Rldi0zNDAxNDM1OC5va3RhLmNvbSIsImF1ZCI6Imh0dHBzOi8vZGV2LTM0MDE0MzU4Lm9rdGEuY29tIiwic3ViIjoiMG9hajd6NXlzazk2aU1wNlU1ZDciLCJpYXQiOjE3MjUwNDMwNTYsImV4cCI6MTcyNTA0NjY1NiwiY2lkIjoiMG9hajd6NXlzazk2aU1wNlU1ZDciLCJ1aWQiOiIwMHVqN3NrOWVic1I4aUlsWDVkNyIsInNjcCI6WyJvcGVuaWQiLCJlbWFpbCIsInByb2ZpbGUiXSwiYXV0aF90aW1lIjoxNzI1MDQyMTk1fQ.GSv2QCY08UjZcAs7_-QBsEYl1GnanleJ7kXG9JuV-TPG6tuhPi-giZTG4PMCoeHRXjYQXetbRziSrsoPaljGDPTW4Piut8mgI4GSCWeWon17dP8LrfWf_uRaYCLxAsYnnTb0u8K9Z00Xalyz_jeozycrywHYoDGhIDzK8NTj8ec7upLrf4sjdovTW0Ak872RV1YG2OpTJEP14s7QhZ_xaQwZVGXpBZU9XL8YrqLs-RRsdCAf9jqRTu3upPDQ_Ytxx--FdqpBNKAyMZnjT9bdAftUX39PernueLM8lfCeHA3aCPeV4OW0j-i--iOkG5pPpDY3s_J4-Vx0y3PrFeDmig",
     "scope": "openid email profile",
     "id_token": "eyJraWQiOiJnQ1EtWDFxV1ZoYWFnUmV5T0xyMWQ2LVhLd1ZTNjRYX0FXQjBQX01mUkhFIiwiYWxnIjoiUlMyNTYifQ.eyJzdWIiOiIwMHVqN3NrOWVic1I4aUlsWDVkNyIsIm5hbWUiOiJHdXN0YXZvVGVzdCBQZXJlaXJhVGVzdCIsImVtYWlsIjoiZ3VndXBlcmVpcmExMjM0QGdtYWlsLmNvbSIsInZlciI6MSwiaXNzIjoiaHR0cHM6Ly9kZXYtMzQwMTQzNTgub2t0YS5jb20iLCJhdWQiOiIwb2FqN3o1eXNrOTZpTXA2VTVkNyIsImlhdCI6MTcyNTA0MzA1NiwiZXhwIjoxNzI1MDQ2NjU2LCJqdGkiOiJJRC5qc0YyRnR0WmtlaFk4bzVqYlR3aU15YkpHc0xDM3pQUW9YVG5CUG9Xck9FIiwiYW1yIjpbInB3ZCJdLCJpZHAiOiIwMG9qNGFobHh4WHR3bnFWczVkNyIsImF1dGhfdGltZSI6MTcyNTA0MjE5NSwiYXRfaGFzaCI6IjE5Wi1SdTQ2MUJPN3hQa0tKeFVEbWcifQ.MSmlGhURlsvzCHazNlGuipwEyxPA6ClKeDWqiYquS-w8OKqKcLXHsuOehCVWK422kUzrTRPgeETWtJVsz19tppwubS7rSTkH48ITgynoj0sBrAV-7VR8N0ItjWwa4EoijLXNfKUcIq7WCK-0KCt_1dpqpC4HxYlz-YAsGJiAsJMswP2eq_NT8lTbfhbyZdrcaeD4u666oqNEMRlQz6BYKNnrxbTcoVvWeSANNFDooFHLBBFUvqH6QkvngFFjDr69hOBE_fnW5n0UEkxLJuBbUd7IoIfR_ZGCr_tODP5IsZtYjZC580bFWzXJbvTKk1mR2X8bfdDT52eVlOPUEzYE3g"
   }
 */