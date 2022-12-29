using System;
using System.Linq.Expressions;

namespace Muffin.Common.DataTables
{
    public interface IDataTableFilterProvider { }
    public interface IDataTableFilterProvider<T> : IDataTableFilterProvider
    {
        void ConfigureTableFilter(IDataTableFilterBuilder<T> builder, string query);
    }

    public interface IDataTableFilterBuilder<T>
    {
        IServiceProvider ServiceProvider { get; set; }
        Expression<Func<T, bool>> FilterExpression { get; set; }
    }

    public abstract class DataTableFilterBuilder<T> : IDataTableFilterBuilder<T>
    {
        public IServiceProvider ServiceProvider { get; set; }
        public Expression<Func<T, bool>> FilterExpression { get; set; }
    }

    public class DefaultDataTableFilterBuilder<T> : DataTableFilterBuilder<T> { }
}
