namespace Pollux.Application.Services
{
    using AutoMapper;
    using Pollux.Common.Application.Models.Response;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IUserPreferencesService
    {
        Task Save(string userId, List<UserPreferenceModel> userPreferenceModel);

        Task<List<UserPreferenceModel>> GetAll(string userId);
    }

    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly IUserPreferencesRepository userPreferencesRepository;
        private readonly IMapper mapper;

        public UserPreferencesService(IUserPreferencesRepository userPreferencesRepository, IMapper mapper)
        {
            this.userPreferencesRepository = userPreferencesRepository;
            this.mapper = mapper;
        }

        /// <summary>Saves the specified user identifier.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userPreferencesModel">The user preferences model.</param>
        /// <returns>Task.</returns>
        public async Task Save(string userId, List<UserPreferenceModel> userPreferencesModel)
        {
            var preferencesDb = await this.userPreferencesRepository.GetManyAsync(p => p.UserId == userId);

            if (preferencesDb != null && preferencesDb.Any())
            {
                foreach (var preferenceDb in preferencesDb)
                {
                    var preferenceModel = userPreferencesModel.Single(p => p.Key == preferenceDb.Key);
                    preferenceDb.Value = preferenceModel.Value;
                    this.userPreferencesRepository.Update(preferenceDb);
                }
            }
            else
            {
                var preferencesNew = this.mapper.Map<List<UserPreferenceModel>, List<UserPreferences>>(userPreferencesModel);
                preferencesNew.ForEach(p =>
                {
                    p.UserId = userId;
                    this.userPreferencesRepository.Add(p);
                });
            }

            await this.userPreferencesRepository.SaveAsync();
        }

        /// <summary>Gets all.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>List<UserPreferenceModel/></returns>
        public async Task<List<UserPreferenceModel>> GetAll(string userId)
        {
            var preferencesDb = await this.userPreferencesRepository.GetManyAsync(p => p.UserId == userId);

            if (preferencesDb == null)
            {
                return null;
            }

            return this.mapper.Map<List<UserPreferences>, List<UserPreferenceModel>>(preferencesDb);
        }
    }
}
