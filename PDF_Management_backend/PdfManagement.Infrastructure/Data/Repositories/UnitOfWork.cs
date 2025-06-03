using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Unit of work implementation for transaction management
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        /// <inheritdoc/>
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<TEntity>(_context);
            }
            
            return (IRepository<TEntity>)_repositories[type];
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <inheritdoc/>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <inheritdoc/>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            
            _disposed = true;
        }
    }
}
