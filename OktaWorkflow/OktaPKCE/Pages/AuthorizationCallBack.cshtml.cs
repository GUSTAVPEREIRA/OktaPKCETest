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
     "access_token": "eyJraWQiOiJnQ1EtWDFxV1ZoYWFnUmV5T0xyMWQ2LVhLd1ZTNjRYX0FXQjBQX01mUkhFIiwidHlwIjoiYXBwbGljYXRpb25cL29rdGEtaW50ZXJuYWwtYXQrand0IiwiYWxnIjoiUlMyNTYifQ.eyJ2ZXIiOjEsImp0aSI6IkFULmlCamszNHd2RVU3eXRPc1VRUTg3SWJGem1mcVJhV2lZS1cwOTk4LUhuT00iLCJpc3MiOiJodHRwczovL2Rldi0zNDAxNDM1OC5va3RhLmNvbSIsImF1ZCI6Imh0dHBzOi8vZGV2LTM0MDE0MzU4Lm9rdGEuY29tIiwic3ViIjoiMG9hajd6NXlzazk2aU1wNlU1ZDciLCJpYXQiOjE3MjUwNDIxOTcsImV4cCI6MTcyNTA0NTc5NywiY2lkIjoiMG9hajd6NXlzazk2aU1wNlU1ZDciLCJ1aWQiOiIwMHVqN3NrOWVic1I4aUlsWDVkNyIsInNjcCI6WyJvcGVuaWQiXSwiYXV0aF90aW1lIjoxNzI1MDQyMTk1fQ.hxjpT_UMAwYB_BchUTRC3eDLtAslKpGivEvClJO1RTFn_29OlOFUsig-stmYihTh6rxx35_ckqVvulUyUgmEgakWLf0habYqkHo3_9Vn7g6Js1wR6_ZoSm7aLMKidOaigZAcbggcle5-B1cfo2_tH8BF1v3kWQKzTcDW2ui7qFfNlv1uCFLRtLBIROwj-yVFRigq2W63fbFxxvwwprI2jow_SDv8DmDXoBAeWNrFUZjGHqZf7y_sDGw2V4wuVHUIygCFCyQ9NtoXVn-8Fh7s2zOgG0u3tWEl2OFfrw38V5Lo8rMapDRt0D3U32wHo5CyWuiLiBf3CiV7TJfkGT5fBg",
     "scope": "openid",
     "id_token": "eyJraWQiOiJnQ1EtWDFxV1ZoYWFnUmV5T0xyMWQ2LVhLd1ZTNjRYX0FXQjBQX01mUkhFIiwiYWxnIjoiUlMyNTYifQ.eyJzdWIiOiIwMHVqN3NrOWVic1I4aUlsWDVkNyIsInZlciI6MSwiaXNzIjoiaHR0cHM6Ly9kZXYtMzQwMTQzNTgub2t0YS5jb20iLCJhdWQiOiIwb2FqN3o1eXNrOTZpTXA2VTVkNyIsImlhdCI6MTcyNTA0MjE5OCwiZXhwIjoxNzI1MDQ1Nzk4LCJqdGkiOiJJRC52V3FBdml2SENWSzAtUmd3VUdydU9NSnhka295Zk00SHlHQktEOGY5MDI0IiwiYW1yIjpbInB3ZCJdLCJpZHAiOiIwMG9qNGFobHh4WHR3bnFWczVkNyIsImF1dGhfdGltZSI6MTcyNTA0MjE5NSwiYXRfaGFzaCI6Im9pUDhlMW4zLUNxZ1JLS0VuZUdPLUEifQ.YFb9v-9RS3v7yv-H5SJG5bKzdoz6TgJgPDVlGcaVFH_GiW-sYe3NhTCqxGlPhrue16RKQ6T_lY4rhdJwlElOl988Lysa2fevNyc35YfCjKPOGJR7P-kswfb4NjXyVj2v-m8Pr6CJnYUhTISn5B6PHW6Ny7EkCpbrwQgKMM7kWlTP3s7_De1OOhvOlkcVzvy5VNq4d3iEVVyDeQkK8Ludv5ZH9uSqKJHtwtUSp1yBOUv0WjNMEiLAoTUEf4yDHO8SgS7h3iyUxjk_iqN_bHfdBXs9reVD1s2wOGJpnGgR8o2MzXYHFX1O_rUeUm74JDMbqaHO7T9xs0OcHKtcDVw5cQ"
   }
 */