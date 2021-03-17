using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pollux.API.OAuth.Models
{
    using System.Net.Http;

    public abstract class TypedClient
    {
        private readonly HttpClient _client;

        public TypedClient(HttpClient client)
        {
            _client = client;
        }

        public virtual async Task<string> CallApi()
        {
            return await _client.GetStringAsync("test");
        }
    }

    public class TypedUserClient : TypedClient
    {
        public TypedUserClient(HttpClient client) : base(client) { }

    }
}
