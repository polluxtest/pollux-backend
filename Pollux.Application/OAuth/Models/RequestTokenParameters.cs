using System;
using System.Collections.Generic;
using System.Text;

namespace Pollux.Application.OAuth.Models
{
    using IdentityModel.Client;

    public class RequestTokenParameters : ClientCredentialsTokenRequest
    {

        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
