namespace Pollux.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <summary>
    /// IRepository contract for data access layer.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        Task SaveAsync();

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Add(TEntity entity);

        /// <summary>
        /// Anies the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>True/False</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Attach(TEntity entity);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes the specified primary keys.
        /// </summary>
        /// <param name="primaryKeys">The primary keys.</param>
        void Delete(params object[] primaryKeys);

        /// <summary>
        /// Gets the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>Entity.</returns>
        TEntity Get(Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// Gets the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>Entity.</returns>
        TEntity Get(Expression<Func<TEntity, bool>> where, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        /// <summary>
        /// Gets the specified primary keys.
        /// </summary>
        /// <param name="primaryKeys">The primary keys.</param>
        /// <returns>Entity</returns>
        TEntity Get(params object[] primaryKeys);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>Entity List.</returns>
        IEnumerable<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>Entity List.</returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <returns>Entity List.</returns>
        Task<List<TEntity>> GetAllAsync();

        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>Entity List.</returns>
        Task<List<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>Entity.</returns>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>Entity.</returns>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> where, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        /// <summary>
        /// Gets the dynamic query.
        /// </summary>
        /// <typeparam name="T">Entity from DbSet Generic.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="where">The where.</param>
        /// <returns>List of Entities.</returns>
        IQueryable<T> GetDynamicQuery<T>(IQueryable<T> query, string sort, string where = null);

        /// <summary>
        /// Gets the many.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>List of Entities. as Enumerable</returns>
        IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        /// <summary>
        /// Gets the many.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>List of Entities. as Enumerable</returns>
        IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// Gets the many asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>List of Entities.</returns>
        Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// Gets the many asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>List of Entities.</returns>
        Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers);

        /// <summary>
        /// Gets the paged query.
        /// </summary>
        /// <typeparam name="T">Entity DbSet.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="maximumRows">The maximum rows.</param>
        /// <param name="startRowIndex">Start index of the row.</param>
        /// <param name="where">The where.</param>
        /// <returns>Paginated Query.</returns>
        IQueryable<T> GetPagedQuery<T>(IQueryable<T> query, string sort, int maximumRows, int startRowIndex, string where = null);

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns>IQueryable Entity.</returns>
        IQueryable<TEntity> GetQuery();

        /// <summary>
        /// Gets the query as no tracking.
        /// </summary>
        /// <returns>IQueryable Entity.</returns>
        IQueryable<TEntity> GetQueryAsNoTracking();

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Entries the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>Entity</returns>
        EntityEntry<TEntity> Entry(TEntity entity);
    }
}
