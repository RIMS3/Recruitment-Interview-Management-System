using RecruitmentInterviewManagementSystem.Applications.Features.Notifications;
using System.Net.Mail;
using System.Net;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class Notifications : INotifications
    {
        public string TypeService => "Email";
        public async Task<bool> SendRegisterAccount(MessageBody request)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("corporationitlocak@gmail.com", "qyai ezpi jokr utjys");
                    smtp.EnableSsl = true;

                    using (MailMessage message = new MailMessage())
                    {
                        message.From = new MailAddress("corporationitlocak@gmail.com", "ITLocak Corparation");
                        message.To.Add(request.To);
                        message.Subject = request.Subject;
                        message.Body = request.Body;
                        message.IsBodyHtml = true;

                        await smtp.SendMailAsync(message);
                    }
                }

                Console.WriteLine("Email sent successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }

        }
    }
}
