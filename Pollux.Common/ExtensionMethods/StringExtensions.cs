namespace Pollux.Common.ExtensionMethods
{
    using System;
    using Pollux.Common.Application.Models.Auth;

    public static class StringExtensions
    {
        public static TokenModel DecodeToken(this string str)
        {
            string[] elements = str.Split(",");
            var accessToken = elements[0].Split(":")[1];
            var refreshToken = elements[1].Split(":")[1];
            string[] expiration = elements[2].Split(":");
            var expirationDateStr = $"{expiration[1]}:{expiration[2]}:{expiration[3]}";

            return new TokenModel()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpirationDate = Convert.ToDateTime(expirationDateStr),
            };
        }
    }
}
