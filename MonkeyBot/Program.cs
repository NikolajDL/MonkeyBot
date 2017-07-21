﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MonkeyBot.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

public class Program
{
    private CommandService commands;
    private DiscordSocketClient client;
    private ServiceCollection serviceCollection;
    private IServiceProvider services;

    private static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

    public async Task Start()
    {
        DiscordSocketConfig discordConfig = new DiscordSocketConfig();
        discordConfig.LogLevel = LogSeverity.Error;
        discordConfig.MessageCacheSize = 400;
        client = new DiscordSocketClient(discordConfig);
        CommandServiceConfig commandConfig = new CommandServiceConfig();
        commandConfig.CaseSensitiveCommands = false;
        commandConfig.DefaultRunMode = RunMode.Async;
        commandConfig.LogLevel = LogSeverity.Error;
        commandConfig.ThrowOnError = true;        
        commands = new CommandService(commandConfig);

        //string token = "***REMOVED***"; // Productive
        string token = "***REMOVED***"; // testing

        serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAnnouncementService>(new AnnouncementService());
        services = serviceCollection.BuildServiceProvider();

        await InstallCommands();

        client.UserJoined += Client_UserJoined;
        client.Connected += Client_Connected;
        client.Log += (l) => Console.Out.WriteLineAsync(l.ToString());

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Client_Connected()
    {
        Console.WriteLine("Connected");
        return Task.CompletedTask;
    }

    private async Task Client_UserJoined(SocketGuildUser arg)
    {
        var channel = arg.Guild.DefaultChannel;
        await channel.SendMessageAsync("Hello there " + arg.Mention + "! Welcome to Monkey-Gamers. Read our welcome page for rules and info. If you have any issues feel free to contact our Admins or Leaders."); //Welcomes the new user
    }

    public async Task InstallCommands()
    {
        // Hook the MessageReceived Event into our Command Handler
        client.MessageReceived += HandleCommand;

        // Discover all of the commands in this assembly and load them.
        await commands.AddModulesAsync(Assembly.GetEntryAssembly());
    }

    public async Task HandleCommand(SocketMessage messageParam)
    {
        // Don't process the command if it was a System Message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;
        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;
        // Determine if the message is a command, based on if it starts with '!' or a mention prefix
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
        // Create a Command Context
        var context = new CommandContext(client, message);
        // Execute the command. (result does not indicate a return value,
        // rather an object stating if the command executed successfully)
        var result = await commands.ExecuteAsync(context, argPos, services);
        if (!result.IsSuccess)
            await context.Channel.SendMessageAsync(result.ErrorReason);
    }
}