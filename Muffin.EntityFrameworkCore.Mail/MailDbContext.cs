using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Muffin.EntityFrameworkCore.Mail
{
    public interface IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> : IDisposable
        where TSendMailJob : class, ISendMailJob
        where TSmtpCredential : class, ISmtpCredential
        where TMailTemplate : class, IMailTemplate
        where TSendMailJobRecipient : class, ISendMailJobRecipient
        where TSendMailJobError : class, ISendMailJobError
    {
        DbSet<TSmtpCredential> SmtpCredentials { get; set; }
        DbSet<TMailTemplate> MailTemplates { get; set; }
        DbSet<TSendMailJob> SendMailJobs { get; set; }
        DbSet<TSendMailJobError> SendMailJobErrors { get; set; }
        DbSet<TSendMailJobRecipient> SendMailJobRecipients { get; set; }

        EntityEntry<TEntity> Add<TEntity>([NotNull] TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Update<TEntity>([NotNull] TEntity entity) where TEntity : class;
        int SaveChanges();
    }

    // public interface IMailDbContext : IMailDbContext<SendMailJob, SmtpCredential, MailTemplate, SendMailJobRecipient, SendMailJobError> { }

    public static class MailDbContextExtensions
    {
        public static void AddMailStore<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>(IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
            where TSendMailJob : SendMailJob
            where TSmtpCredential : SmtpCredential
            where TMailTemplate : MailTemplate
            where TSendMailJobRecipient : SendMailJobRecipient
            where TSendMailJobError : SendMailJobError
        {
            services.AddDbContext<TContext>(optionsAction);
            services.AddScoped<IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>, TContext>();
        }
    }
}
