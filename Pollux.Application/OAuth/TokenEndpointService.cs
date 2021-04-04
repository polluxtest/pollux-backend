// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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

    using Pollux.Application.OAuth.Models;
    using Pollux.Common.Application.Models.Request;

    /// <summary>
    /// Implements token endpoint operations using IdentityModel
    /// </summary>
    public class TokenEndpointService : ITokenEndpointService
    {
        private readonly ITokenClientConfigurationService _configService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenEndpointService> _logger;


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configService"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="logger"></param>
        public TokenEndpointService(
            ITokenClientConfigurationService configService,
            IHttpClientFactory httpClientFactory,
            ILogger<TokenEndpointService> logger)
        {
            _configService = configService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RequestClientAccessToken(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName,
            LogInModel loginModel = null,
            ClientAccessTokenParameters parameters = null,
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
        public async Task<TokenResponse> RefreshUserAccessTokenAsync(
        string refreshToken,
        UserAccessTokenParameters parameters = null,
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
            UserAccessTokenParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Revoking refresh token: {token}", refreshToken);

            parameters ??= new UserAccessTokenParameters();

            var requestDetails = await _configService.GetTokenRevocationRequestAsync(parameters);
            requestDetails.Token = refreshToken;
            requestDetails.TokenTypeHint = OidcConstants.TokenTypes.RefreshToken;

#if NET5_0
            requestDetails.Options.TryAdd(AccessTokenManagementDefaults.AccessTokenParametersOptionsName, parameters);
#elif NETCOREAPP3_1
            requestDetails.Properties[AccessTokenManagementDefaults.AccessTokenParametersOptionsName] = parameters;
#endif

            var httpClient = _httpClientFactory.CreateClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
            return await httpClient.RevokeTokenAsync(requestDetails, cancellationToken);
        }
    }
}