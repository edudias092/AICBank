
using System.Linq.Expressions;
using AICBank.Core.Entities;

namespace AICBank.Data.Repositories
{
    public interface IRepository<T> : IDisposable where T : Entity
    {
        Task<T> GetById(int id);
        Task<List<T>> GetAll();
        Task<List<T>> Get(Expression<Func<T, bool>> expression);
        Task Add(T entity);
        Task Update(T entity);
        Task Remove(int id);
        Task Commit();
    }
}