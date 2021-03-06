﻿using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonkeyBot.Database;
using MonkeyBot.Models;
using System;
using System.Threading.Tasks;

namespace MonkeyBot.Services
{
    public class MonkeyClient : DiscordSocketClient
    {
        private readonly ILogger<MonkeyClient> logger;
        private readonly MonkeyDBContext dbContext;

        private static readonly DiscordSocketConfig discordConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Warning,
            MessageCacheSize = 1000
        };

        public MonkeyClient(ILogger<MonkeyClient> logger, MonkeyDBContext dbContext) : base(discordConfig)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            Connected += Client_ConnectedAsync;
            UserJoined += Client_UserJoinedAsync;
            UserLeft += Client_UserLeftAsync;
            JoinedGuild += Client_JoinedGuildAsync;
            LeftGuild += Client_LeftGuildAsync;
            GuildMemberUpdated += Client_GuildMemberUpdateAsync;
            Log += MonkeyClient_LogAsync;
        }

        private Task Client_ConnectedAsync()
        {
            logger.LogInformation("Connected");
            return Task.CompletedTask;
        }

        private async Task Client_UserJoinedAsync(SocketGuildUser user)
        {
            if (user.Guild == null)
            {
                return;
            }

            GuildConfig config = await dbContext.GuildConfigs.SingleOrDefaultAsync(c => c.GuildID == user.Guild.Id).ConfigureAwait(false);
            string welcomeMessage = config?.WelcomeMessageText ?? string.Empty;
            ITextChannel channel = user.Guild.DefaultChannel;
            if (config?.WelcomeMessageChannelId != null)
            {
                channel = user.Guild.GetTextChannel(config.WelcomeMessageChannelId) ?? user.Guild.DefaultChannel;
            }
            if (!welcomeMessage.IsEmpty())
            {
                welcomeMessage = welcomeMessage.Replace("%server%", user.Guild.Name).Replace("%user%", user.Mention);
                _ = await (channel?.SendMessageAsync(welcomeMessage)).ConfigureAwait(false);
            }
        }

        private async Task Client_UserLeftAsync(SocketGuildUser user)
        {
            if (user.Guild == null)
            {
                return;
            }

            GuildConfig config = await dbContext.GuildConfigs.SingleOrDefaultAsync(c => c.GuildID == user.Guild.Id).ConfigureAwait(false);
            string goodbyeMessage = config?.GoodbyeMessageText ?? string.Empty;
            ITextChannel channel = user.Guild.DefaultChannel;
            if (config?.GoodbyeMessageChannelId != null)
            {
                channel = user.Guild.GetTextChannel(config.GoodbyeMessageChannelId) ?? user.Guild.DefaultChannel;
            }
            if (!goodbyeMessage.IsEmpty())
            {
                goodbyeMessage = goodbyeMessage.Replace("%server%", user.Guild.Name).Replace("%user%", user.Username);
                _ = await (channel?.SendMessageAsync(goodbyeMessage)).ConfigureAwait(false);
            }
        }

        private async Task Client_JoinedGuildAsync(SocketGuild guild)
        {
            logger.LogInformation($"Joined guild {guild.Name}");
            GuildConfig config = await dbContext.GuildConfigs.SingleOrDefaultAsync(c => c.GuildID == guild.Id).ConfigureAwait(false);
            if (config == null)
            {
                config = new GuildConfig();
                _ = dbContext.GuildConfigs.Add(config);
                _ = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task Client_LeftGuildAsync(SocketGuild guild)
        {
            logger.LogInformation($"Left guild {guild.Name}");
            GuildConfig config = await dbContext.GuildConfigs.SingleOrDefaultAsync(c => c.GuildID == guild.Id).ConfigureAwait(false);
            if (config != null)
            {
                _ = dbContext.GuildConfigs.Remove(config);
                _ = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberUpdateAsync(SocketGuildUser before, SocketGuildUser after)
        {
            GuildConfig config = await dbContext.GuildConfigs.SingleOrDefaultAsync(c => c.GuildID == after.Guild.Id).ConfigureAwait(false);
            if (config == null || !config.StreamAnnouncementsEnabled || config.ConfirmedStreamerIds == null || !config.ConfirmedStreamerIds.Contains(after.Id))
            {
                // Streaming announcements has to be enabled for the guild and the streamer must first opt in to have it announced
                return;
            }

            if (before?.Activity?.Type != ActivityType.Streaming && after?.Activity?.Type == ActivityType.Streaming && after.Activity is StreamingGame stream)
            {
                ITextChannel channel = after.Guild.DefaultChannel;
                if (config?.DefaultChannelId != null)
                {
                    channel = after.Guild.GetTextChannel(config.GoodbyeMessageChannelId) ?? after.Guild.DefaultChannel;
                }
                _ = await (channel?.SendMessageAsync($"{after.Username} has started streaming. Watch it [here]({stream.Url}) ")).ConfigureAwait(false);
            }
        }

        private async Task MonkeyClient_LogAsync(LogMessage logMessage)
        {
            string msg = $"{logMessage.Source}: {logMessage.Message}";
            Exception ex = logMessage.Exception;
            if (logMessage.Severity <= LogSeverity.Warning && ConnectionState == ConnectionState.Connected && !ex.Message.Contains("WebSocket connection was closed", StringComparison.OrdinalIgnoreCase))
            {
                string adminMessage = $"{msg} {ex?.Message}";
                await NotifyAdminAsync(adminMessage).ConfigureAwait(false);
            }
            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    logger.LogCritical(ex, msg);
                    break;

                case LogSeverity.Error:
                    logger.LogError(ex, msg);
                    break;

                case LogSeverity.Warning:
                    logger.LogWarning(ex, msg);
                    break;

                case LogSeverity.Info:
                    logger.LogInformation(ex, msg);
                    break;

                case LogSeverity.Verbose:
                    logger.LogTrace(ex, msg);
                    break;

                case LogSeverity.Debug:
                    logger.LogDebug(ex, msg);
                    break;

                default:
                    break;
            }
            return;
        }

        public Task NotifyAdminAsync(string adminMessage)
        {
            SocketUser adminuser = GetUser(327885109560737793);
            return (adminuser?.SendMessageAsync(adminMessage));
        }
    }
}