using System;

namespace Muffin.EntityFrameworkCore.Mail.Services
{
    public class EmailSenderQueueEvents
    {
        public event EmailSenderEvent OnEnqueue;
        public event EmailWorkerEvent OnDequeue;
        public event EmailSenderErrorEvent OnError;

        internal void InvokeOnEnqueue(object sender, ISmtpCredential smtpCredential, IMailTemplate mailTemplate, ISendMailJob sendMailJob)
        {
            OnEnqueue?.Invoke(sender, new EmailSenderEventArgs()
            {
                SmtpCredential = smtpCredential,
                MailTemplate = mailTemplate,
                SendMailJob = sendMailJob
            });
        }

        internal void InvokeOnDequeue(object sender, ISmtpCredential smtpCredential, IMailTemplate mailTemplate, ISendMailJob sendMailJob)
        {
            OnDequeue?.Invoke(sender,new EmailWorkerEventArgs()
            {
                SmtpCredential = smtpCredential,
                MailTemplate = mailTemplate,
                SendMailJob = sendMailJob
            });
        }

        internal void InvokeOnError(object sender, ISmtpCredential smtpCredential, Exception exception)
        {
            OnError?.Invoke(sender, new EmailSenderErrorEventArgs()
            {
                SmtpCredential = smtpCredential,
                Exception = exception
            });
        }
    }

    public delegate void EmailSenderErrorEvent(object sender, EmailSenderErrorEventArgs args);
    public class EmailSenderErrorEventArgs
    {
        public ISmtpCredential SmtpCredential { get; set; }
        public Exception Exception { get; set; }
    }

    public delegate void EmailSenderEvent(object sender, EmailSenderEventArgs args);
    public class EmailSenderEventArgs
    {
        public ISmtpCredential SmtpCredential { get; set; }
        public IMailTemplate MailTemplate { get; set; }
        public ISendMailJob SendMailJob { get; set; }
    }

    public delegate void EmailWorkerEvent(object sender, EmailWorkerEventArgs args);
    public class EmailWorkerEventArgs
    {
        public ISmtpCredential SmtpCredential { get; set; }
        public IMailTemplate MailTemplate { get; set; }
        public ISendMailJob SendMailJob { get; set; }
    }


}
