using BotFatality.System;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;

namespace BotFatality
{
    internal class Bot
    {
        private static DiscordSocketClient _client;

        public static SocketGuild guild;

        static void Main(string[] args)
        {

            Task.Run(async () => { await Startup(); }).Wait();
        }

        public static async Task Startup()
        {
            Console.WriteLine("Progarm started");

            _client = new DiscordSocketClient();

            _client.Log += Log;

            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.Ready += Client_Ready;
            _client.ModalSubmitted += ModalReceived;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = "NDgyNTUzMTYyOTE0NTk0ODI5.W4ASpw.6iq2JRjhiW5z0acfBQ_dguSodyQ";

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public static async Task Client_Ready()
        {
            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
            var ticket_cmd = new SlashCommandBuilder();

            guild = _client.GetGuild(908091527539097601);
            #region Ticket
            ticket_cmd.WithName("ticket");

            ticket_cmd.WithDescription("Créer un ticket pour avoir l'asssistance d'un membre du staff");
            #endregion
            #region AddPlayer
            var addOption = new SlashCommandOptionBuilder()
                    .WithName("joueur")
                    .WithDescription("Le membre à ajouter au ticket")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Mentionable);

                var ticket_add = new SlashCommandBuilder()
                    .WithName("add")
                    .WithDescription("Ajouter un membre au ticket. Seules les staffs ont accès à cette commande")
                    .AddOption(addOption);
                #endregion
            #region RemovePlayer
                var removeOption = new SlashCommandOptionBuilder()
                    .WithName("joueur")
                    .WithDescription("Le membre à retirer au ticket")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Mentionable);


                var ticket_remove = new SlashCommandBuilder()
                   .WithName("remove")
                   .WithDescription("Retirer un membre du ticket. Seules les staffs ont accès à cette commande")
                   .AddOption(removeOption);
            #endregion
            #region TakeTicket
            var takeOption = new SlashCommandOptionBuilder()
                .WithName("prevenir")
                .WithDescription("Prevenir le joueur que le ticket est en cours de traitement ?")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Boolean);


            var ticket_take = new SlashCommandBuilder()
               .WithName("take")
               .WithDescription("Prendre le ticket. Seules les staffs ont accès à cette commande")
               .AddOption(takeOption);
            #endregion
            #region TicketInfo
            var ticket_info = new SlashCommandBuilder()
               .WithName("info")
               .WithDescription("Avoir les info du ticket. Seules les staffs ont accès à cette commande");
            #endregion

            #region TicketClose
            var ticket_close = new SlashCommandBuilder()
               .WithName("close")
               .WithDescription("Fermer ticket");
            #endregion
            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                await _client.CreateGlobalApplicationCommandAsync(ticket_cmd.Build());
                await _client.CreateGlobalApplicationCommandAsync(ticket_add.Build());
                await _client.CreateGlobalApplicationCommandAsync(ticket_remove.Build());
                await _client.CreateGlobalApplicationCommandAsync(ticket_take.Build());
                await _client.CreateGlobalApplicationCommandAsync(ticket_info.Build());
                await _client.CreateGlobalApplicationCommandAsync(ticket_close.Build());
                await Ticket.InitSystem();

            }
            catch (Exception exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Message, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }
        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (command.Data.Name == "ticket")
            {
                await Ticket.OnCommand(command);
            }
            if (command.Data.Name == "add")
            {
                await Ticket.AddPlayer(command);
            }
            if (command.Data.Name == "remove")
            {
                await Ticket.RemovePlayer(command);
            }
            if (command.Data.Name == "take")
            {
                await Ticket.TakeTicket(command);
            }
            if (command.Data.Name == "info")
            {
                await Ticket.TicketInfo(command);
            }
            if (command.Data.Name == "close")
            {
                await Ticket.TicketClose(command);
            }
        }

        private static async Task ModalReceived(SocketModal arg)
        {
            await Ticket.OnModal(arg);
        }

        private static async Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
        }
    }
}