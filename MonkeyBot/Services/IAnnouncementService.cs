﻿using MonkeyBot.Announcements;
using System;

namespace MonkeyBot.Services
{
    /// <summary>Interface for a minimum announcement service that handles single and recurring announcements</summary>
    public interface IAnnouncementService
    {
        /// <summary>Holds all announcements</summary>
        AnnouncementList Announcements { get; }

        /// <summary>
        /// A method that will add an announcement that should be broadcasted regularly based on the interval defined by the Cron Expression
        /// It will require a way to schedule to announcement
        /// </summary>
        /// <param name="ID">The unique ID of the announcement</param>
        /// <param name="cronExpression">The cron expression that defines the broadcast intervall.</param>
        /// <param name="message">The message to be broadcasted</param>
        /// <param name="message">The ID of the Guild where the message will be broadcasted</param>
        void AddRecurringAnnouncement(string ID, string cronExpression, string message, ulong guildID);

        /// <summary>
        /// A method that will add an announcement that should be broadcasted once on the provided Execution Time
        /// It will require a way to schedule to announcement
        /// </summary>
        /// <param name="ID">The unique ID of the announcement</param>
        /// <param name="excecutionTime">The date and time at which the message should be broadcasted.</param>
        /// <param name="message">The message to be broadcasted</param>
        /// <param name="message">The ID of the Guild where the message will be broadcasted</param>
        void AddSingleAnnouncement(string ID, DateTime excecutionTime, string message, ulong guildID);

        /// <summary>A method that provides a way to remove an announcement</summary>
        void Remove(string announcementID, ulong guildID);

        /// <summary>
        /// A method that returns the next execution time of the announcement with the provided ID
        /// </summary>
        /// <returns>Next execution time</returns>
        DateTime GetNextOccurence(string announcementID, ulong guildID);

        /// <summary>A method that provides a way to load persisted announcements</summary>
        void LoadAnnouncements();

        /// <summary>A method that provides a way to persist announcements</summary>
        void SaveAnnouncements();
    }
}