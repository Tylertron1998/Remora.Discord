//
//  DiscordRestWebhookAPI.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.Core;
using Remora.Discord.Rest.Extensions;
using Remora.Discord.Rest.Results;
using Remora.Discord.Rest.Utility;
using Remora.Results;

namespace Remora.Discord.Rest.API.Webhooks
{
    /// <inheritdoc />
    public class DiscordRestWebhookAPI : IDiscordRestWebhookAPI
    {
        private readonly DiscordHttpClient _discordHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestWebhookAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The json options.</param>
        public DiscordRestWebhookAPI(DiscordHttpClient discordHttpClient, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _discordHttpClient = discordHttpClient;
            _jsonOptions = jsonOptions.Value;
        }

        /// <inheritdoc />
        public async Task<ICreateRestEntityResult<IWebhook>> CreateWebhookAsync
        (
            Snowflake channelID,
            string name,
            Stream? avatar,
            CancellationToken ct = default
        )
        {
            if (name.Length < 1 || name.Length > 80)
            {
                return CreateRestEntityResult<IWebhook>.FromError("Names must be between 1 and 80 characters");
            }

            if (name.Equals("clyde", StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateRestEntityResult<IWebhook>.FromError("Names cannot be \"clyde\".");
            }

            string? avatarDataString = null;
            if (!(avatar is null))
            {
                var packAvatar = await ImagePacker.PackImageAsync(avatar, ct);
                if (!packAvatar.IsSuccess)
                {
                    return CreateRestEntityResult<IWebhook>.FromError(packAvatar);
                }

                avatarDataString = packAvatar.Entity;
            }

            return await _discordHttpClient.PostAsync<IWebhook>
            (
                $"channels/{channelID}/webhook",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.WriteString("avatar", avatarDataString);
                    }
                ),
                ct
            );
        }

        /// <inheritdoc />
        public Task<IRetrieveRestEntityResult<IReadOnlyList<IWebhook>>> GetChannelWebhooksAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IWebhook>>
            (
                $"channels/{channelID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<IRetrieveRestEntityResult<IReadOnlyList<IWebhook>>> GetGuildWebhooksAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IWebhook>>
            (
                $"guilds/{guildID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<IRetrieveRestEntityResult<IWebhook>> GetWebhookAsync
        (
            Snowflake webhookID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IWebhook>
            (
                $"webhooks/{webhookID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<IRetrieveRestEntityResult<IWebhook>> GetWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IWebhook>
            (
                $"webhooks/{webhookID}/{token}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public async Task<IModifyRestEntityResult<IWebhook>> ModifyWebhookAsync
        (
            Snowflake webhookID,
            Optional<string> name = default,
            Optional<Stream?> avatar = default,
            Optional<Snowflake> channelID = default,
            CancellationToken ct = default
        )
        {
            Optional<string?> avatarData = default;
            if (avatar.HasValue)
            {
                if (avatar.Value is null)
                {
                    avatarData = new Optional<string?>(null);
                }
                else
                {
                    var packImage = await ImagePacker.PackImageAsync(avatar.Value, ct);
                    if (!packImage.IsSuccess)
                    {
                        return ModifyRestEntityResult<IWebhook>.FromError(packImage);
                    }

                    avatarData = packImage.Entity;
                }
            }

            return await _discordHttpClient.PatchAsync<IWebhook>
            (
                $"webhooks/{webhookID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("avatar", avatarData, _jsonOptions);
                        json.Write("channel_id", channelID, _jsonOptions);
                    }
                ),
                ct
            );
        }

        /// <inheritdoc />
        public async Task<IModifyRestEntityResult<IWebhook>> ModifyWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            Optional<string> name = default,
            Optional<Stream?> avatar = default,
            CancellationToken ct = default
        )
        {
            Optional<string?> avatarData = default;
            if (avatar.HasValue)
            {
                if (avatar.Value is null)
                {
                    avatarData = new Optional<string?>(null);
                }
                else
                {
                    var packImage = await ImagePacker.PackImageAsync(avatar.Value, ct);
                    if (!packImage.IsSuccess)
                    {
                        return ModifyRestEntityResult<IWebhook>.FromError(packImage);
                    }

                    avatarData = packImage.Entity;
                }
            }

            return await _discordHttpClient.PatchAsync<IWebhook>
            (
                $"webhooks/{webhookID}/{token}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("avatar", avatarData, _jsonOptions);
                    }
                ),
                ct
            );
        }

        /// <inheritdoc />
        public Task<IDeleteRestEntityResult> DeleteWebhookAsync(Snowflake webhookID, CancellationToken ct = default)
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{webhookID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<IDeleteRestEntityResult> DeleteWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{webhookID}/{token}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<ICreateRestEntityResult<IMessage>> ExecuteWebhookAsync
        (
            Snowflake webhookID,
            string token,
            Optional<bool> shouldWait = default,
            Optional<string> content = default,
            Optional<string> username = default,
            Optional<string> avatarUrl = default,
            Optional<bool> isTTS = default,
            Optional<Stream> file = default,
            Optional<IReadOnlyList<IEmbed>> embeds = default,
            Optional<IAllowedMentions> allowedMentions = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PostAsync<IMessage>
            (
                $"webhooks/{webhookID}/{token}",
                b =>
                {
                    if (shouldWait.HasValue)
                    {
                        b.AddQueryParameter("wait", shouldWait.Value.ToString());
                    }

                    if (file.HasValue)
                    {
                        b.AddContent(new StreamContent(file.Value), "file");
                    }

                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("content", content, _jsonOptions);
                            json.Write("username", username, _jsonOptions);
                            json.Write("avatar_url", avatarUrl, _jsonOptions);
                            json.Write("tts", isTTS, _jsonOptions);
                            json.Write("embeds", embeds, _jsonOptions);
                            json.Write("allowed_mentions", allowedMentions, _jsonOptions);
                        }
                    );
                },
                ct
            );
        }
    }
}