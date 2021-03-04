namespace Pollux.Persistence
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Unit of work implementation.
    /// </summary>
    /// <typeparam name="T">Db Context.</typeparam>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="Pollux.Persistence.IUnitOfWork" />
    public abstract class UnitOfWork<T> : IDisposable, IUnitOfWork
        where T : DbContext
    {
        private readonly ILogger<UnitOfWork<T>> logger;

        /// <summary>
        /// Gets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        internal DbContext DbContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{T}"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="logger">The logger.</param>
        public UnitOfWork(T dataContext, ILogger<UnitOfWork<T>> logger)
        {
            this.DbContext = dataContext;
            this.logger = logger;

        }

        /// <summary>
        /// Transactions the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>
        /// True/False.
        /// </returns>
        public bool Transaction(Action action)
        {
            using (var transaction = this.DbContext.Database.BeginTransaction())
            {
                try
                {
                    action();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    this.logger.LogError(default(EventId), ex, ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        /// <returns>
        /// True/False.
        /// </returns>
        public bool Commit()
        {
            try
            {
                this.DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                this.logger.LogError(default(EventId), ex, ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Commits the asynchronous.
        /// </summary>
        /// <returns>
        /// why int?.
        /// </returns>
        public Task<int> CommitAsync()
        {
            try
            {
                return this.DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(default(EventId), ex, ex.Message);
                return Task.FromResult(0);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.DbContext?.Dispose();  // todo check this out
        }
    }
}
