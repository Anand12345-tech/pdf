using Microsoft.EntityFrameworkCore;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PdfManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc/>
        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DeleteByIdAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }
            
            return await DeleteAsync(entity);
        }
    }
}
