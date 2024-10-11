using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private protected readonly CipdbContext _context;

        protected BaseRepository(CipdbContext context)
        {
            _context = context;
        }

        public virtual async Task<List<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T> AddAsync(T obj, CancellationToken cancellationToken)
        {
            return (await _context.Set<T>().AddAsync(obj, cancellationToken)).Entity;
        }

        public virtual T Update(T obj)
        {
            return _context.Set<T>().Update(obj).Entity;
        }

        public virtual T Delete(T obj)
        {
            _context.Set<T>().Remove(obj);
            return obj;
        }

        public virtual void RemoveRange(params T[] entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public virtual async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        }
        public virtual async Task<List<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _context.Set<T>(); 
            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }


        public virtual async Task<IEnumerable<T>> WhereAsync( Expression<Func<T, bool>> predicate,CancellationToken cancellationToken,Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            // Include related entities if specified
            if (include != null)
            {
                query = include(query);
            }

            // Apply the filter predicate
            query = query.Where(predicate);

            // Execute the query and return the results as a list
            return await query.ToListAsync(cancellationToken);
        }

        public IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }
        


    }
}
