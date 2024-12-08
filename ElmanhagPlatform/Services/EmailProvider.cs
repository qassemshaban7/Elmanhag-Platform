using ElmanhagPlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ElmanhagPlatform.Services
{
    public class EmailProvider : IEmailProvider
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public EmailProvider(IConfiguration config, AppDbContext context)
        {
            _context = context;
            _config = config;
        }

        public async Task<int> SendMail( string UserId, string Value)
        {
            var user = await _context.ApplicationUsers.FindAsync(UserId);
            if (user.Email == null) return 0;

            string subject;
            string templatePath;

            subject = "تعديل بيانات الحساب";
            templatePath = Directory.GetCurrentDirectory() + "/wwwroot/Email.html";

            string htmlTemplate = System.IO.File.ReadAllText(templatePath);

            htmlTemplate = htmlTemplate.Replace("MessEMa", user.FullName);
            htmlTemplate = htmlTemplate.Replace("MessEMb", Value);
            htmlTemplate = htmlTemplate.Replace("%%MessEMc%%", user.Id);

            var sender = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == "ecc07b18-f55e-4f6b-95bd-0e84f556135f");

            var message = new MailMessage();
            message.From = new MailAddress( sender.Email);
            message.To.Add(new MailAddress(user.Email));
            message.Subject = subject;
            message.Body = htmlTemplate;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient(_config["stmp:Host"], int.Parse(_config["stmp:Port"])))
            {
                smtp.Credentials = new NetworkCredential(sender.Email, sender.AppsPassword);
                smtp.EnableSsl = true;

                try
                {
                    await smtp.SendMailAsync(message);
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }

            return 0;
        }
    }
}