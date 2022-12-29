namespace Muffin.Mail.Abstraction
{
    public interface IMailTemplateModelPrearatorProvider
    {
        IMailTemplateModelPrearator GetMailTemplateModelPrearator(object obj);
        IMailTemplateModelPrearator<T> GetMailTemplateModelPrearator<T>();
        MailTemplateModelContext GetMailTemplateModelContextDetermineConcreteType(object model);
        MailTemplateModelContext<T> GetMailTemplateModelContext<T>(T model);
    }
}
