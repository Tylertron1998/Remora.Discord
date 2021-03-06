//
//  DiscordRestInteractionAPI.cs
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

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.Core;

namespace Remora.Discord.Rest.API
{
    /// <inheritdoc />
    public class DiscordRestInteractionAPI : IDiscordRestInteractionAPI
    {
        private readonly DiscordHttpClient _discordHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestInteractionAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The json options.</param>
        public DiscordRestInteractionAPI
        (
            DiscordHttpClient discordHttpClient,
            IOptions<JsonSerializerOptions> jsonOptions
        )
        {
            _discordHttpClient = discordHttpClient;
            _jsonOptions = jsonOptions.Value;
        }

        /// <inheritdoc />
        public Task<IRestResult> CreateInteractionResponseAsync
        (
            Snowflake interactionID,
            string interactionToken,
            IInteractionResponse response,
            CancellationToken ct
        )
        {
            return _discordHttpClient.PostAsync
            (
                $"interactions/{interactionID}/{interactionToken}/callback",
                b => b.WithJson
                (
                    json =>
                    {
                        JsonSerializer.Serialize(json, response, _jsonOptions);
                    },
                    false
                ),
                ct: ct
            );
        }
    }
}
