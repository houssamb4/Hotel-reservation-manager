using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Mvc;

namespace Partie_Font_Office.Controllers
{
    public class ContactController : Controller
    {
        [HttpPost]
        public IActionResult SubmitForm(string name, string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Golden Horizon Hotel", "no-reply@golden-horizon.com"));
                emailMessage.To.Add(new MailboxAddress("Admin", "admin@example.com"));
                emailMessage.Subject = subject;

                // HTML email content with inline CSS for styling
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='text-align: center; color: #0056b3;'>New Message from {name}</h2>
                                <p><strong>Name:</strong> {name}</p>
                                <p><strong>Email:</strong> {email}</p>
                                <p><strong>Subject:</strong> {subject}</p>
                                <p><strong>Message:</strong></p>
                                <div style='border: 1px solid #ddd; padding: 15px; background-color: #f9f9f9;'>
                                    <p>{message}</p>
                                </div>
                                <p style='font-size: 12px; color: #999;'>This is an automatically generated email from Golden Horizon Hotel. Please do not reply to this email.</p>
                            </div>
                        </body>
                    </html>"
                };

                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("YOUR_EMAIL_HERE", "YOUR_PASSWORD_HERE");
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                return RedirectToPage("/sent");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
