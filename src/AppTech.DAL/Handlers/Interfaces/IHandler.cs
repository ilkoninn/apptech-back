using AppTech.Core.Entities.Commons;

namespace AppTech.DAL.Handlers.Interfaces
{
    public interface IHandler<T> where T : BaseEntity
    {
        T HandleEntityAsync(T entity);
    }
}
