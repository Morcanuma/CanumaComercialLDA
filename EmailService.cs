using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace lojaCanuma.Services
{
    public static class EmailService
    {
        public static void EnviarComAnexo(string para, string assunto, string corpoHtml, string caminhoPdf)
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(para)) return;
            if (string.IsNullOrWhiteSpace(caminhoPdf) || !File.Exists(caminhoPdf)) return;

            // Lê config do App.config
            var host = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            var portStr = ConfigurationManager.AppSettings["SmtpPort"];
            var user = ConfigurationManager.AppSettings["SmtpUser"];
            var pass = ConfigurationManager.AppSettings["SmtpPass"]; // senha de app
            var fromName = ConfigurationManager.AppSettings["SmtpFrom"] ?? "Loja Canuma";

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("SMTP não configurado: SmtpUser/SmtpPass ausentes.");

            if (!int.TryParse(portStr, out var port)) port = 587;

            // Para depuração (não imprime senha!)
            System.Diagnostics.Debug.WriteLine($"SMTP -> host:{host} port:{port} user:{user} from:{fromName}");

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(user, fromName); // remetente deve ser o mesmo do user
                msg.To.Add(new MailAddress(para));
                msg.Subject = assunto ?? "";
                msg.SubjectEncoding = Encoding.UTF8;
                msg.Body = corpoHtml ?? "";
                msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Attachments.Add(new Attachment(caminhoPdf));

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false; // 👈 importante
                    smtp.EnableSsl = true;              // Gmail usa SSL/TLS
                    smtp.Credentials = new NetworkCredential(user, pass);
                    smtp.Timeout = 15000;

                    // Para evitar problemas em alguns ambientes
                    ServicePointManager.Expect100Continue = true;

                    smtp.Send(msg);
                }
            }
        }
    }
}
