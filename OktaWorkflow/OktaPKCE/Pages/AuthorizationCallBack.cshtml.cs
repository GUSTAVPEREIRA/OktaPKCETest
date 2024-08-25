using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OktaPKCE.Pages;

public class AuthorizationCallBack : PageModel
{
    public const string OktaDomain = "dev-34014358.okta.com";
    public const string TokenEndpoint  = $"{OktaDomain}/oauth2/v1/token";
    
    public void OnGet()
    {
        
    }
}