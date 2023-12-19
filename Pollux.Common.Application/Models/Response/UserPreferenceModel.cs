using System.Collections.Generic;

namespace Pollux.Common.Application.Models.Response
{
    public class UserPreferenceModelResponse
    {
        public Dictionary<string, string> Preferences { get; set; }
    }

    public class UserPreferenceModel
    {
        /// <summary>Gets or sets the key.</summary>
        /// <value>The key.</value>
        public string Key { get; set; }
        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public string Value { get; set; }
    }

}
