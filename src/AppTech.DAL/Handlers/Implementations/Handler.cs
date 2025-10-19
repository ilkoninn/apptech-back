using AppTech.Core.Entities.Commons;
using AppTech.Core.Exceptions.Commons;
using AppTech.DAL.Handlers.Interfaces;

namespace AppTech.DAL.Handlers.Implementations
{
    public class Handler<T> : IHandler<T> where T : BaseEntity
    {
        public T HandleEntityAsync(T entity)
        {
            if (entity is null)
                throw new EntityNotFoundException($"Entity of type {typeof(T).Name} not found in the database.");

            return entity;
        }
    }
}
