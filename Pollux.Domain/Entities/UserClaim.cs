using System;
using System.Collections.Generic;
using System.Text;

namespace Pollux.Domain.Entities
{
    public class UserClaim
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }
    }
}
