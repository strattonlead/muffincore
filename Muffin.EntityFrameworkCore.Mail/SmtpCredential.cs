using Muffin.EntityFrameworkCore.Entity;
using Muffin.EntityFrameworkCore.Entity.Abstraction;

namespace Muffin.EntityFrameworkCore.Mail
{
    public interface ISmtpCredential : IBaseEntity
    {
        string Server { get; set; }
        int Port { get; set; }
        string From { get; set; }
        string DisplayName { get; set; }
        bool UseCredentials { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        bool UseSsl { get; set; }
        int? MaxSendAttempts { get; set; }
    }

    public class SmtpCredential : BaseEntity, ISmtpCredential
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string From { get; set; }
        public string DisplayName { get; set; }
        public bool UseCredentials { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
        public int? MaxSendAttempts { get; set; }
        //public int? MaxConcurrentMailJobs { get; set; }
        //public int? SendLimitPerMinute { get; set; }
    }
}
