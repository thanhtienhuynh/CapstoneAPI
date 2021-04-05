using CapstoneAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        internal readonly CapstoneDBContext _context;
        internal DbSet<T> dbSet;
        public string errorMsg = string.Empty;
        public GenericRepository(CapstoneDBContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }
        public virtual async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filter = null,
                Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                string includeProperties = "",
                int first = 0, int offset = 0)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (first > 0)
            {
                query = query.Take(first);
            }
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (offset > 0)
            {
                return await query.Skip(offset).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }
        public virtual async Task<T> GetById(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual void Insert(T entity)
        {
            if (entity == null) throw new ArgumentException("entity");
            dbSet.Add(entity);
        }
        public virtual void Update(T entity)
        {
            if (entity == null) throw new ArgumentException("entity");
            dbSet.Attach(entity);
            dbSet.Update(entity);
        }
        public virtual void Delete(object id)
        {
            T entity = dbSet.Find(id);
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }

        public virtual void DeleteComposite(Expression<Func<T, bool>> filter = null)
        {
            IEnumerable<T> entities = dbSet.Where(filter);
            dbSet.AttachRange(entities);
            dbSet.RemoveRange(entities);
        }

        public Task<T> GetFirst(Expression<Func<T, bool>> filter = null, string includeProperties = "")
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split
               (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            return query.FirstOrDefaultAsync();
        }
    }
}
