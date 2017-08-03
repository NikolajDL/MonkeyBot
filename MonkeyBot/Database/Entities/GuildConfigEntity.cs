﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MonkeyBot.Database.Entities
{
    public class GuildConfigEntity : BaseEntity
    {
        [Required]
        [Column]
        public ulong GuildId { get; set; }

        [Required]
        public string CommandPrefix { get; set; }

        public string WelcomeMessageText { get; set; }

        [Column("Rules")]
        public string RulesAsString { get; set; }

        [NotMapped]
        public List<string> Rules
        {
            get { return RulesAsString.Split(';').ToList(); }
            set
            {
                RulesAsString = string.Join(";", value);
            }
        }
    }
}