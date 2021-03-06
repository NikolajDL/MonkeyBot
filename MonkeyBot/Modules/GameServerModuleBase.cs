﻿using Discord.Commands;
using Microsoft.Extensions.Logging;
using MonkeyBot.Common;
using MonkeyBot.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    public abstract class GameServerModuleBase : MonkeyModuleBase
    {
        private readonly IGameServerService gameServerService;
        private readonly ILogger<ModuleBase> logger;

        protected GameServerModuleBase(IGameServerService gameServerService, ILogger<ModuleBase> logger)
        {
            this.gameServerService = gameServerService;
            this.logger = logger;
        }

        protected async Task AddGameServerInternalAsync(string ip, ulong channelID)
        {
            //Do parameter checks
            IPEndPoint endPoint = await ParseIPAsync(ip).ConfigureAwait(false);
            if (endPoint == null)
            {
                return;
            }
            bool success = false;
            try
            {
                // Add the Server to the Service to activate it
                success = await gameServerService.AddServerAsync(endPoint, Context.Guild.Id, channelID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _ = await ReplyAsync($"There was an error while adding the game server:{Environment.NewLine}{ex.Message}").ConfigureAwait(false);
                logger.LogWarning(ex, "Error adding a gameserver");
            }
            _ = success
                ? await ReplyAsync("GameServer added").ConfigureAwait(false)
                : await ReplyAsync("GameServer could not be added").ConfigureAwait(false);
        }

        protected async Task RemoveGameServerInternalAsync(string ip)
        {
            //Do parameter checks
            IPEndPoint endPoint = await ParseIPAsync(ip).ConfigureAwait(false);
            if (endPoint == null)
            {
                return;
            }

            try
            {
                // Remove the server from the Service
                await gameServerService.RemoveServerAsync(endPoint, Context.Guild.Id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _ = await ReplyAsync($"There was an error while trying to remove the game server:{Environment.NewLine}{ex.Message}").ConfigureAwait(false);
                logger.LogWarning(ex, "Error removing a gameserver");
            }
            _ = await ReplyAsync("GameServer removed").ConfigureAwait(false);
        }

        //TODO: move the parsing into the command handler
        private async Task<IPEndPoint> ParseIPAsync(string ip)
        {
            string[] splitIP = ip?.Split(':');
            if (ip.IsEmpty() || splitIP == null || splitIP.Length != 2 || !IPAddress.TryParse(splitIP[0], out IPAddress parsedIP) || !int.TryParse(splitIP[1], out int port))
            {
                _ = await ReplyAsync("You need to specify an IP-Adress + Port for the server! For example 127.0.0.1:1234").ConfigureAwait(false);
                return null;
            }
            var endPoint = new IPEndPoint(parsedIP, port);
            return endPoint;
        }
    }
}