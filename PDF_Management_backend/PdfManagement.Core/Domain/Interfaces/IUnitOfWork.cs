using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace PdfManagement.Core.Domain.Interfaces
{
    /// <summary>
    /// Unit of work interface for transaction management
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the database context
        /// </summary>
        DbContext Context { get; }
        
        /// <summary>
        /// Gets a repository for a specific entity type
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Repository for the entity type</returns>
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        
        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>Transaction object</returns>
        Task BeginTransactionAsync();
        
        /// <summary>
        /// Commits the transaction
        /// </summary>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
