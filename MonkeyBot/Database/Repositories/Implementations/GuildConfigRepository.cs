﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MonkeyBot.Common;
using MonkeyBot.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Database.Repositories
{
    public class GuildConfigRepository : BaseGuildRepository<GuildConfigEntity, GuildConfig>, IGuildConfigRepository
    {
        public GuildConfigRepository(DbContext context) : base(context)
        {
        }

        public async Task<GuildConfig> GetAsync(ulong guildId)
        {
            var dbConfig = await dbSet.SingleOrDefaultAsync(x => (ulong)x.GuildId == guildId).ConfigureAwait(false);
            if (dbConfig == null)
                return null;
            return Mapper.Map<GuildConfig>(dbConfig);
        }

        public override async Task AddOrUpdateAsync(GuildConfig obj)
        {
            var dbCfg = await dbSet.FirstOrDefaultAsync(x => x.GuildId == obj.GuildId).ConfigureAwait(false);
            if (dbCfg == null)
            {
                dbSet.Add(dbCfg = new GuildConfigEntity
                {
                    GuildId = obj.GuildId,
                    Rules = obj.Rules.ToList(),
                    CommandPrefix = obj.CommandPrefix,
                    WelcomeMessageText = obj.WelcomeMessageText,
                    WelcomeMessageChannelId = obj.WelcomeMessageChannelId,
                    GoodbyeMessageText = obj.GoodbyeMessageText,
                    GoodbyeMessageChannelId = obj.GoodbyeMessageChannelId
                });
            }
            else
            {
                dbCfg.GuildId = obj.GuildId;
                dbCfg.Rules = new List<string>(obj.Rules);
                dbCfg.CommandPrefix = obj.CommandPrefix;
                dbCfg.WelcomeMessageText = obj.WelcomeMessageText;
                dbCfg.WelcomeMessageChannelId = obj.WelcomeMessageChannelId;
                dbCfg.GoodbyeMessageText = obj.GoodbyeMessageText;
                dbCfg.GoodbyeMessageChannelId = obj.GoodbyeMessageChannelId;
                dbSet.Update(dbCfg);
            }
        }

        public override async Task RemoveAsync(GuildConfig obj)
        {
            if (obj == null)
                return;
            var entity = await dbSet.FirstOrDefaultAsync(x => x.GuildId == obj.GuildId).ConfigureAwait(false);
            if (entity != null)
                dbSet.Remove(entity);
        }
    }
}