
using System.Collections.Generic;
using IdentityModel.Client;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    using System.Linq;

    using AutoMapper.Internal;

    using IdentityServer4;
    using IdentityServer4.Models;

    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants;

    public interface ITokenIdentityService
    {
        /// <summary>
        /// Refreshes a user access token.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a client access token.
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TokenResponse> RequestClientAccessToken(string clientName = null, LogInModel loginModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implements token endpoint operations using IdentityModel
    /// </summary>
    public class TokenIdentityService : ITokenIdentityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenIdentityService> _logger;


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configService"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="logger"></param>
        public TokenIdentityService(
            IHttpClientFactory httpClientFactory,
            ILogger<TokenIdentityService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RequestClientAccessToken(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName,
            LogInModel loginModel = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Requesting client access token for client: {client}", clientName);

            var clientCredentials = new IdentityModel.Client.PasswordTokenRequest()
            {
                Method = HttpMethod.Post,
                Address = "http://localhost:5000/connect/token",
                ClientId = "client",
                ClientSecret = "secret",
                GrantType = GrantTypes.ResourceOwnerPassword.First(),
                Scope = "api api/pollux offline_access",
                UserName = loginModel.Email,
                Password = loginModel.Password,
            };

            var httpClient = _httpClientFactory.CreateClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
            return await httpClient.RequestPasswordTokenAsync(clientCredentials);
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        /// <inheritdoc/>
        /// <inheritdoc/>
        public async Task<TokenResponse> RefreshUserAccessTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Refreshing refresh token: {token}", refreshToken);

            var refreshTokenRequest = new RefreshTokenRequest()
            {
                Method = HttpMethod.Post,
                Address = "http://localhost:5000/connect/token",
                ClientId = "client",
                ClientSecret = "secret",
                GrantType = "refresh_token",
                RefreshToken = refreshToken
            };

            //var requestDetails = await _configService.GetRefreshTokenRequestAsync(parameters);
            // requestDetails.RefreshToken = refreshToken;

            var s = _httpClientFactory.CreateClient("hola");
            var httpClient = _httpClientFactory.CreateClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
            var response = await httpClient.RequestRefreshTokenAsync(refreshTokenRequest);
            return response;
        }




        /// <inheritdoc/>
        public async Task<TokenRevocationResponse> RevokeRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Revoking refresh token: {token}", refreshToken);


            var httpClient = _httpClientFactory.CreateClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
            //return await httpClient.RevokeTokenAsync(requestDetails, cancellationToken);

            return new TokenRevocationResponse();
        }
    }
}