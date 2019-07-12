﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MonkeyBot.Database.Entities;
using MonkeyBot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Database.Repositories
{
    public class GameSubscriptionRepository : BaseGuildRepository<GameSubscriptionEntity, GameSubscription>, IGameSubscriptionRepository
    {
        public GameSubscriptionRepository(DbContext context) : base(context)
        {
        }

        public override async Task AddOrUpdateAsync(GameSubscription obj)
        {
            var gameSubscription = await dbSet.FirstOrDefaultAsync(x => x.GuildId == obj.GuildId && x.UserId == obj.UserId && x.GameName == obj.GameName).ConfigureAwait(false);
            if (gameSubscription == null)
            {
                dbSet.Add(gameSubscription = new GameSubscriptionEntity
                {
                    GuildId = obj.GuildId,
                    UserId = obj.UserId,
                    GameName = obj.GameName
                });
            }
            else
            {
                gameSubscription.GuildId = obj.GuildId;
                gameSubscription.UserId = obj.UserId;
                gameSubscription.GameName = obj.GameName;
            }
        }

        public async Task<List<GameSubscription>> GetAllForUserAsync(ulong userId)
        {
            var gameSubscription = await dbSet.Where(x => x.UserId == userId).ToListAsync().ConfigureAwait(false);
            if (gameSubscription == null)
                return null;
            return Mapper.Map<List<GameSubscription>>(gameSubscription);
        }

        public override async Task RemoveAsync(GameSubscription obj)
        {
            if (obj == null)
                return;
            var entity = await dbSet.FirstOrDefaultAsync(x => x.GuildId == obj.GuildId && x.UserId == obj.UserId && x.GameName == obj.GameName).ConfigureAwait(false);
            if (entity != null)
                dbSet.Remove(entity);
        }
    }
}