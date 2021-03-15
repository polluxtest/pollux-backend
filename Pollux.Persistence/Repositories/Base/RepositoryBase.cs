namespace Pollux.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using LinqKit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <summary>
    /// Implementation of Repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Pollux.Persistence.IRepository{TEntity}" />
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// The dbset.
        /// </summary>
        protected readonly DbSet<TEntity> dbSet;

        /// <summary>
        /// The database context.
        /// </summary>
        protected DbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected RepositoryBase(DbContext context)
        {
            this.dbContext = context;
            this.dbSet = this.dbContext.Set<TEntity>();
        }

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Add(TEntity entity)
        {
            this.dbSet.Add(entity);
        }

        /// <summary>
        /// Anies the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>
        /// True/False
        /// </returns>
        public virtual bool Any(Expression<Func<TEntity, bool>> where)
        {
            return this.dbSet.Any(where);
        }

        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Attach(TEntity entity)
        {
            if (this.dbContext.Entry(entity).State == EntityState.Detached)
            {
                this.dbSet.Attach(entity);
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntity entity)
        {
            this.dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes the specified primary keys.
        /// </summary>
        /// <param name="primaryKeys">The primary keys.</param>
        public virtual void Delete(params object[] primaryKeys)
        {
            var entity = this.Get(primaryKeys);
            if (entity != null)
            {
                this.dbSet.Remove(entity);
            }
        }

        /// <summary>
        /// Gets the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>
        /// Entity.
        /// </returns>
        public virtual TEntity Get(Expression<Func<TEntity, bool>> where)
        {
            return this.dbSet.Where(where).SingleOrDefault();
        }

        /// <summary>
        /// Gets the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>
        /// Entity.
        /// </returns>
        public TEntity Get(
          Expression<Func<TEntity, bool>> where,
          Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return includeMembers(this.dbSet.Where(where)).SingleOrDefault();
        }

        /// <summary>
        /// Gets the specified primary keys.
        /// </summary>
        /// <param name="primaryKeys">The primary keys.</param>
        /// <returns>
        /// Entity.
        /// </returns>
        public virtual TEntity Get(params object[] primaryKeys)
        {
            return this.dbSet.Find(primaryKeys);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>
        /// Entity List.
        /// </returns>
        public virtual IEnumerable<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return includeMembers(this.dbSet).ToList();
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>
        /// Entity List.
        /// </returns>
        public virtual IEnumerable<TEntity> GetAll()
        {
            return this.dbSet.ToList();
        }

        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <returns>
        /// Entity List.
        /// </returns>
        public Task<List<TEntity>> GetAllAsync()
        {
            return this.dbSet.ToListAsync();
        }

        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>
        /// Entity List.
        /// </returns>
        public Task<List<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return includeMembers(this.dbSet).ToListAsync();
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>
        /// Entity.
        /// </returns>
        public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> where)
        {
            return this.dbSet.Where(where).AsNoTracking().SingleOrDefaultAsync();
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>
        /// Entity.
        /// </returns>
        public Task<TEntity> GetAsync(
          Expression<Func<TEntity, bool>> where,
          Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return includeMembers(this.dbSet.Where(where)).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Gets the dynamic query.
        /// </summary>
        /// <typeparam name="T">Entity from DbSet Generic.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="where">The where.</param>
        /// <returns>
        /// List of Entities.
        /// </returns>
        public virtual IQueryable<T> GetDynamicQuery<T>(IQueryable<T> query, string sort, string where = null)
        {
            if (string.IsNullOrEmpty(where))
            {
                where = "true";
            }

            query = query.AsExpandable().Where(where);
            return DynamicQueryableExtensions.OrderBy(query, sort);
        }

        /// <summary>
        /// Gets the many.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>
        /// List of Entities. as Enumerable.
        /// </returns>
        public virtual IEnumerable<TEntity> GetMany(
          Expression<Func<TEntity, bool>> where,
          Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return includeMembers(this.dbSet.Where(where)).ToList();
        }

        /// <summary>
        /// Gets the many.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>
        /// List of Entities. as Enumerable
        /// </returns>
        public virtual IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where)
        {
            return this.dbSet.Where(where).ToList();
        }

        /// <summary>
        /// Gets the many asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>
        /// List of Entities.
        /// </returns>
        public Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where)
        {
            return this.dbSet.Where(where).ToListAsync();
        }

        /// <summary>
        /// Gets the many asynchronous.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="includeMembers">The include members.</param>
        /// <returns>
        /// List of Entities.
        /// </returns>
        public Task<List<TEntity>> GetManyAsync(
          Expression<Func<TEntity, bool>> where,
          Func<IQueryable<TEntity>, IQueryable<TEntity>> includeMembers)
        {
            return includeMembers(this.dbSet.Where(where)).ToListAsync();
        }

        /// <summary>
        /// Gets the paged query.
        /// </summary>
        /// <typeparam name="T">Entity DbSet.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="maximumRows">The maximum rows.</param>
        /// <param name="startRowIndex">Start index of the row.</param>
        /// <param name="where">The where.</param>
        /// <returns>
        /// Paginated Query.
        /// </returns>
        public virtual IQueryable<T> GetPagedQuery<T>(
          IQueryable<T> query,
          string sort,
          int maximumRows,
          int startRowIndex,
          string where = null)
        {
            var a = this.GetDynamicQuery(query, sort, where).Skip(1).Take(2);
            return this.GetDynamicQuery(query, sort, where).Skip(startRowIndex).Take(maximumRows);
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns>
        /// IQueryable Entity.
        /// </returns>
        public virtual IQueryable<TEntity> GetQuery()
        {
            return this.dbSet;
        }

        /// <summary>
        /// Gets the query as no tracking.
        /// </summary>
        /// <returns>
        /// IQueryable Entity.
        /// </returns>
        public virtual IQueryable<TEntity> GetQueryAsNoTracking()
        {
            this.dbSet.GetHashCode();
            return this.dbSet.AsNoTracking();
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Update(TEntity entity)
        {
            this.Attach(entity);
            this.dbContext.Update(entity);
        }

        /// <summary>
        /// Entries the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// Entity
        /// </returns>
        public virtual EntityEntry<TEntity> Entry(TEntity entity)
        {
            return this.dbContext.Entry(entity);
        }
    }
}
