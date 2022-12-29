namespace Muffin.EntityFrameworkCore.Entity.Abstraction
{
    public interface IBaseEntity<T>
    {
        T Id { get; set; }
    }

    public interface IBaseEntity : IBaseEntity<long> { }
}
