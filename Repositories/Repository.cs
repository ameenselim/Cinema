using Cinema_Project.Repositories;
using CinemaProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cinema_Project.Repositories
{
      public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<int> Commit(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred while saving changes: {ex.Message}");
                return 0; // Return 0 to indicate that no changes were saved
            }
        }
        public async Task<IEnumerable<T>> GetAsync(
                 Expression<Func<T, bool>>? expression = null,
                 Expression<Func<T, object>>?[]? includes = null,
                 Func<IQueryable<T>, IQueryable<T>>? thenInclude = null,
                 bool Tracking = true,
                 CancellationToken cancellationToken = default)

        {
            IQueryable<T> entities = _dbSet.AsQueryable();

            if (expression is not null)
                entities = entities.Where(expression);

            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    if (item is not null)
                        entities = entities.Include(item);
                }
            }

            if (thenInclude is not null)
            {
                entities = thenInclude(entities);
            }

            if (!Tracking)
                entities = entities.AsNoTracking();

            return await entities.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>> expression,
            Expression<Func<T, object>>?[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? thenInclude = null,
            bool Tracking = true,
            CancellationToken cancellationToken = default
            )
        {
            var entity = _dbSet.AsQueryable();

            if (includes is not null)
                foreach (var item in includes)
                    if (item is not null)
                        entity = entity.Include(item);

            if(thenInclude is not null)
            {
                entity = thenInclude(entity);
            }

            if (!Tracking)
                entity = entity.AsNoTracking();

            return await entity.FirstOrDefaultAsync(expression, cancellationToken);
        }
    }
}
