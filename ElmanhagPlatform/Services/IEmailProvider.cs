namespace ElmanhagPlatform.Services
{
    public interface IEmailProvider
    {
        Task<int> SendMail( string UserId, string Value);
    }
}
