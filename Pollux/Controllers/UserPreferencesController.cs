﻿namespace Pollux.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Application.Services;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Application.Models.Response;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [AllowAnonymous]
    public class UserPreferencesController : BaseController
    {
        private readonly IUserPreferencesService userPreferencesService;

        public UserPreferencesController(IUserPreferencesService userPreferencesService)
        {
            this.userPreferencesService = userPreferencesService;
        }

        /// <summary>
        /// Insert User Preferences.
        /// </summary>
        /// <param name="userPreferences">The User Preferences Model</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Post([FromBody] UserPreferencesPostModel userPreferences)
        {
            await this.userPreferencesService.Save(userPreferences.UserId, userPreferences.Preferences);

            return this.Ok();
        }

        /// <summary>Gets the specified user identifier.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>List<UserPreferenceModel/></returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<UserPreferenceModel>>> Get([FromQuery] string userId)
        {
            var preferences = await this.userPreferencesService.GetAll(userId);

            return this.Ok(preferences);
        }
    }
}