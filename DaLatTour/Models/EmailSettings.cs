using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;


namespace DaLatTour.Models
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
    }

}