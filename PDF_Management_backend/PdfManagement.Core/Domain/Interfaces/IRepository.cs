using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PdfManagement.Core.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetByIdAsync(object id);
        
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Collection of entities</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Finds entities based on a predicate
        /// </summary>
        /// <param name="predicate">Search predicate</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAsync(T entity);
        
        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteAsync(T entity);
        
        /// <summary>
        /// Deletes an entity by its ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteByIdAsync(object id);
    }
}
