using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CipdbContext _context;

        private IBaseRepository<User>? _userRepository;
        private IBaseRepository<Organize>? _organizeRepository;
        private IBaseRepository<Major>? _majorRepository;

        public UnitOfWork(CipdbContext context)
        {
            _context = context;
        }
        internal CipdbContext Context => _context;

        public IBaseRepository<User> UserRepository => _userRepository ??= new UserRepository(_context);
        public IBaseRepository<Organize> OrganizeRepository => _organizeRepository ??= new OrganizeRepository(_context);
        public IBaseRepository<Major> MajorRepository => _majorRepository ??= new MajorRepository(_context);



        public async Task Commit(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}
