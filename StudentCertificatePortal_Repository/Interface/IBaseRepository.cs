using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Interface
{
    public interface IBaseRepository<T> where T : class
    {
        Task<List<T>> GetAll();
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T obj, CancellationToken cancellationToken = default);
        T Update(T obj);
        T Delete(T obj);
        Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        void RemoveRange(params T[] entities);

        Task<List<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>> include = null);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, Func<IQueryable<T>, IQueryable<T>> include = null);
        IQueryable<T> Include(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken, Func<IQueryable<T>, IQueryable<T>> include = null);

        Task<int> CountAsync(CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    }
}
