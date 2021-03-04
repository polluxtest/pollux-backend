namespace Pollux.Persistence
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit of Work interface definition.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Commits this instance.
        /// </summary>
        /// <returns>True/False.</returns>
        bool Commit();

        /// <summary>
        /// Transactions the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>True/False.</returns>
        bool Transaction(Action action);

        // todo check this out , why returning and int
        /// <summary>
        /// Commits the asynchronous.
        /// </summary>
        /// <returns>why int?</returns>
        Task<int> CommitAsync();
    }
}
