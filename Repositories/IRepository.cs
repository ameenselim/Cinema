using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cinema_Project.Repositories
{
    public interface IRepository<T>
    {
        Task CreateAsync(T entity, CancellationToken cancellationToken = default);
         void Update(T entity);

        void Delete(T entity);
        Task<int> Commit(CancellationToken cancellationToken = default);
         Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? thenInclude = null,
            bool Tracking = true,
            CancellationToken cancellationToken = default
            );


        Task<T?> GetOneAsync(
            Expression<Func<T, bool>> expression,
            Expression<Func<T, object>>?[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? thenInclude = null,
            bool Tracking = true,
            CancellationToken cancellationToken = default
            );
    }
}
