using System;

namespace Muffin.Common.DataTables
{
    public interface IDataTableDescriptionProvider { }
    public interface IDataTableDescriptionProvider<T> : IDataTableDescriptionProvider
    {
        void ConfigureTableDescription(IDataTableDescriptionBuilder<T> builder);
    }

    public interface IDataTableDescriptionBuilder<T>
    {
        IServiceProvider ServiceProvider { get; set; }
        DataTableDescription<T> DataTableDescription { get; set; }
    }

    public abstract class DataTableDescriptionBuilder<T> : IDataTableDescriptionBuilder<T>
    {
        public IServiceProvider ServiceProvider { get; set; }
        public DataTableDescription<T> DataTableDescription { get; set; } = new DataTableDescription<T>();
    }

    public class DefaultDataTableDescriptionBuilder<T> : DataTableDescriptionBuilder<T> { }
}
