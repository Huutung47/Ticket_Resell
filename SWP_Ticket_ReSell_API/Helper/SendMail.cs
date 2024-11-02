using System.Net;
using System.Net.Mail;

namespace SWP_Ticket_ReSell_API.Helper
{
    public class SendMail
    {
        private readonly IConfiguration _configuration;
        public SendMail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool SendEMail(string to, string subject, string body, string attachFile)
        {
            string emailSender = _configuration["EmailSettings:EmailSender"];
            string hostEmail = _configuration["EmailSettings:HostEmail"];
            int portEmail = int.Parse(_configuration["EmailSettings:Port"]);
            string passwordSender = _configuration["EmailSettings:Password"];

            try
            {
                MailMessage msg = new MailMessage(emailSender, to, subject, body)
                {
                    IsBodyHtml = true
                };
                using (var client = new SmtpClient(hostEmail, portEmail))
                {
                    client.EnableSsl = true;
                    if (!string.IsNullOrEmpty(attachFile))
                    {
                        Attachment attachment = new Attachment(attachFile);
                        msg.Attachments.Add(attachment);
                    }
                    NetworkCredential credential = new NetworkCredential(emailSender, passwordSender);
                    client.UseDefaultCredentials = false;
                    client.Credentials = credential;
                    client.Send(msg);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
