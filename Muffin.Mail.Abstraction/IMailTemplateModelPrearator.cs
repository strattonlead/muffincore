namespace Muffin.Mail.Abstraction
{
    public interface IMailTemplateModelPrearator
    {
        void Prepare(MailTemplateModelContext context);
    }
    public interface IMailTemplateModelPrearator<T> : IMailTemplateModelPrearator
    {
        void Prepare(MailTemplateModelContext<T> context);
    }
}