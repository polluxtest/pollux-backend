namespace Pollux.Application.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Pollux.Common.Application.Models.Response;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;

    public interface IUserPreferencesService
    {
        Task Save(string userId, List<UserPreferenceModel> userPreferenceModel);

        Task<UserPreferenceModelResponse> GetAll(string userId);
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
        /// <returns>Dic<UserPreferenceModel/></returns>
        public async Task<UserPreferenceModelResponse> GetAll(string userId)
        {
            var preferencesDb = await this.userPreferencesRepository.GetManyAsync(p => p.UserId == userId);

            if (preferencesDb == null)
            {
                return null;
            }

            var preferencesHash = preferencesDb.ToDictionary(p => p.Key, p => p.Value);

            return new UserPreferenceModelResponse { Preferences = preferencesHash.Count == 0 ? null : preferencesHash };
        }
    }
}
