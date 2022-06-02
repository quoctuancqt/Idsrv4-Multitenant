// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Demo
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                        new IdentityResources.Email(),
                        new IdentityResources.Phone(),
                        new IdentityResources.Address(),
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("oauth.api","Identity Server API") { Description = "This scope allows application to communicate with Identity Server" },
                new ApiScope("games-api","Games API") { Description = "This scope allows application to communicate with Games API" },
            };

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("oauth.api", "Identity API")
                {
                    ApiSecrets = { new Secret("oauth.api".Sha256()) },

                    Scopes = { "oauth.api" }
                },
                new ApiResource("games-api", "Games API")
                {
                    ApiSecrets = { new Secret("games-api".Sha256()) },

                    Scopes = { "games-api" }
                }
            };
        }

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "web",
                    ClientName = "Web",
                    ClientSecrets = { new Secret("90fb49d8-e1ec-45d5-86d2-c4f430d36fd7".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://notuse" },
                    PostLogoutRedirectUris = { "https://notuse" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "email", "oauth.api" }
                },
                new Client
                {
                    AllowedGrantTypes = GrantTypes.Implicit,
                    ClientName = "Games List",
                    ClientId = "941b8aa0-0085-47ad-84da-73340390d946",
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "games-api",
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address
                    },
                    RedirectUris = new List<string>()
                    {
                        "http://{0}.local:4200/",
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "http://{0}.local:4200/",
                    },
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret("games-secret".Sha512())
                    }
                }
            };
    }
}