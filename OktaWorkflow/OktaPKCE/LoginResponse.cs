namespace OktaPKCE.Controllers;

public class LoginResponse
{
    public string CodeVerifier { get; set; }
    public string OktaRedirect { get; set; }
}