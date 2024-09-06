namespace Pollux.Common.Application.Models.Request
{
    using Response;
    using System.Collections.Generic;

    public class UserPreferencesPostModel
    {
        public string UserId { get; set; }

        public List<UserPreferenceModel> Preferences { get; set; }
    }
}
