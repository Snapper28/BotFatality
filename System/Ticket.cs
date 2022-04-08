using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Converters;
using System.Text;

namespace BotFatality.System
{
    public static class Ticket
    {
        public static string fullpath = null;

        public static List<TicketData> tickets = new List<TicketData>();

        public static async Task InitSystem()
        {
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                if (!Directory.Exists(Path.Combine(appdata, "FatalityBot")))
                {
                    Directory.CreateDirectory(Path.Combine(appdata, "FatalityBot"));
                }

                var dirpath = Path.Combine(appdata, "FatalityBot");

                if (!File.Exists(Path.Combine(dirpath, "id.txt")))
                {
                    File.Create(Path.Combine(dirpath, "id.txt"));
                }

                fullpath = Path.Combine(dirpath, "id.txt");


                var data = await File.ReadAllTextAsync(fullpath);

                if (data == "")
                {
                    File.WriteAllText(fullpath, "0");
                }

                if (!File.Exists(Path.Combine(dirpath, "data.txt")))
                {
                    File.Create(Path.Combine(dirpath, "data.txt"));
                }

                var dpath = Path.Combine(dirpath, "data.txt");

                string d = String.Empty;

                using(var ddata = new FileStream(dpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        ddata.CopyTo(memoryStream);
                        d = Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }

                if (d != "")
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    tickets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TicketData>>(d);
#pragma warning restore CS8601 // Possible null reference assignment.
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public static async Task SaveData()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!Directory.Exists(Path.Combine(appdata, "FatalityBot")))
            {
                Directory.CreateDirectory(Path.Combine(appdata, "FatalityBot"));
            }

            var dirpath = Path.Combine(appdata, "FatalityBot");

            if (!File.Exists(Path.Combine(dirpath, "data.txt")))
            {
                File.Create(Path.Combine(dirpath, "data.txt"));
            }

            var dpath = Path.Combine(dirpath, "data.txt");

            var ddata = await File.ReadAllTextAsync(dpath);

            var t = Newtonsoft.Json.JsonConvert.SerializeObject(tickets);

            await File.WriteAllTextAsync(dpath, t);
        }

        public static async Task<int> GetId()
        {
            var t = int.Parse(await File.ReadAllTextAsync(fullpath));

            await File.WriteAllTextAsync(fullpath, $"{t+1}");

            return t;
        }

        public static TicketData GetTicket(int id)
        {
            for (int i = 0; i < tickets.Count; i++)
            {
                if(tickets[i].ticketID == id)
                {
                    return tickets[i];
                }
            }

            return null;
        }

        public static int? GetInt(string s)
        {
            string b = string.Empty;

            for (int i = 0; i < s.Length; i++)
            {
                if (Char.IsDigit(s[i]))
                    b += s[i];
            }

            if (b.Length > 0)
                return int.Parse(b);
            return null;
        }

        public static string GetStatus(TicketData ticket)
        {
            if (ticket.ticketStatus == Status.pending)
            {
                return "En attente";
            }
            else if (ticket.ticketStatus == Status.taken)
            {
                return $"Pris par {ticket.staffID}";
            }
            else
            {
                return "Fermeé";
            }
        }

        public static async Task<string> GetStaff(TicketData ticket, IMessageChannel channel)
        {
            if(ticket.staffID == 0)
            {
                return "Aucun";
            }
            else
            {
                var s = await channel.GetUserAsync(ticket.staffID);
                if(s != null)
                {
                    return s.Mention;
                }
                else
                {
                    return "Impossible de trouver le staff. Il a probablement plus sont grade";
                }
            }
        }
        public static async Task<string> GetUser(TicketData ticket, IMessageChannel channel)
        {
            var s = await channel.GetUserAsync(ticket.userID);

            if (s != null)
            {
                return s.Mention;
            }
            else
            {
                return "Impossible de trouver le joueur. Il a probablement plus les access au ticket";
            }
        }
        public static async Task<string> GetUserName(TicketData ticket, IMessageChannel channel)
        {
            var s = await channel.GetUserAsync(ticket.userID);

            if (s != null)
            {
                return s.Username;
            }
            else
            {
                return "Impossible de trouver le joueur. Il a probablement plus les access au ticket";
            }
        }
        public static async Task TicketClose(SocketSlashCommand cmd)
        {
            var c = await cmd.GetChannelAsync();
            if (!c.Name.Contains("logs"))
            {
                var staffRole = Bot.guild.GetRole(942507484982157402);
                if (((SocketGuildUser)cmd.User).Roles.Contains(staffRole))
                {

                    TicketData ticket = GetTicket(GetInt(c.Name).Value);
                    var us = ticket.userID;
                    if(us != 0)
                    {
                        var u = await c.GetUserAsync(ticket.userID);
                        if (cmd.User == u)
                        {
                            await u.SendMessageAsync($"Vous avez fermé votre ticket (ID: {ticket.ticketID})\nSi vous avez d'autres questions n'hesitez pas a faire un autre ticket");
                        }
                        else
                        {
                            await u.SendMessageAsync($"Votre ticket(ID: {ticket.ticketID}) a été fermé par un membre du staff\nSi vous avez d'autres questions n'hesitez pas a faire un autre ticket");
                        }
                    }
                    else
                    {

                    }

                    await cmd.DeferAsync();

                    await Bot.guild.GetTextChannel(c.Id).ModifyAsync(p => { p.Name = $"logs-{ticket.ticketType}-{ticket.ticketID}"; p.CategoryId = 961902889553514536; });

                    ticket.ticketStatus = Status.closed;

                    await c.SendMessageAsync($"Le ticket a été fermée par {cmd.User.Mention}");

                    await cmd.RespondAsync($"{cmd.User.Mention}, Vous avez fermée le ticket. N'oubliez pas d'enlever les joueurs avec */remove*", ephemeral: true);
                }
                else
                {
                    await cmd.RespondAsync("Vous n'êtes pas un membre du staff");
                }
            }
            else if (!c.Name.Contains("ticket") && !c.Name.Contains("traitement"))
            {
                await cmd.RespondAsync("Vous n'êtes pas dans le bon salon", ephemeral: true);
            }
            else
            {
                await cmd.RespondAsync("Ce ticket est deja fermee", ephemeral: true);
            }
        }

        public static async Task TakeTicket(SocketSlashCommand cmd)
        {
            var c = await cmd.GetChannelAsync();
            if (c.Name.Contains("ticket"))
            {
                var staffRole = Bot.guild.GetRole(942507484982157402);
                if (((SocketGuildUser)cmd.User).Roles.Contains(staffRole))
                {
                    var c_name = c.Name;

                    TicketData ticket = GetTicket(GetInt(c.Name).Value);

                    await Bot.guild.GetTextChannel(c.Id).ModifyAsync(p => p.Name = $"traitement-{ticket.ticketType}-{ticket.ticketID}");

                    bool tell = (bool)cmd.Data.Options.First().Value;


                    if (tell)
                    {

                        var user = await c.GetUserAsync(ticket.userID);

                        await c.SendMessageAsync($"{user.Mention}, Votre ticket est en cours de traitement");
                    }
                    else
                    {
                        await cmd.RespondAsync(":thumbsup:", ephemeral: true);
                    }

                    ticket.staffID = cmd.User.Id;
                    ticket.ticketStatus = Status.taken;

                    await cmd.DeferAsync();
                    await SaveData();

                }
                else
                {
                    await cmd.RespondAsync("Vous n'êtes pas un membre du staff");
                }
            }
            else if (c.Name.Contains("traitement"))
            {
                TicketData ticket = GetTicket(GetInt(c.Name).Value);
                await cmd.RespondAsync($"Ce ticket est deja pris par {(await c.GetUserAsync(ticket.staffID)).Mention}", ephemeral: true);
            }
            else
            {
                await cmd.RespondAsync("Vous n'êtes pas dans le bon salon", ephemeral: true);
            }
        }

        public static async Task TicketInfo(SocketSlashCommand cmd)
        {
            var c = await cmd.GetChannelAsync();
            if (c.Name.Contains("ticket") || c.Name.Contains("traitement") || c.Name.Contains("closed"))
            {
                var staffRole = Bot.guild.GetRole(942507484982157402);
                if (((SocketGuildUser)cmd.User).Roles.Contains(staffRole))
                {

                    var ticket = GetTicket((GetInt(c.Name).Value));

                    var e = new EmbedBuilder();
                        e.WithTitle($"Information du ticket de {await GetUserName(ticket, c)} ID: {ticket.ticketID}");
                        e.AddField("Nom IG", ticket.NameIG, true);
                        e.AddField("Type", ticket.ticketType, true);
                        e.AddField("Problème", ticket.ticketIssue, false);
                        e.AddField("Autre informations", ticket.ticketOtherInfo, false);
                        e.AddField("Statut", GetStatus(ticket), true);
                        e.AddField("Staff", await GetStaff(ticket, c), true);
                        e.AddField("Mention du Joueur", await GetUser(ticket, c), false);
                        e.AddField("Date", ticket.ticketDate, false);
                        e.WithColor(Color.Blue);

                    var embed = e.Build();

                    await cmd.RespondAsync(embed: embed, ephemeral: true);

                }
                else
                {
                    await cmd.RespondAsync("Vous n'êtes pas un membre du staff");
                }
            }
            else
            {
                await cmd.RespondAsync("Vous n'êtes pas dans le bon salon", ephemeral: true);
            }
        }

        public static async Task AddPlayer(SocketSlashCommand cmd)
        {
            var c = await cmd.GetChannelAsync();
            if (c.Name.Contains("ticket") || c.Name.Contains("traitement"))
            {
                var staffRole = Bot.guild.GetRole(942507484982157402);

                if (((SocketGuildUser)cmd.User).Roles.Contains(staffRole))
                {
                    await Bot.guild.GetTextChannel(c.Id).AddPermissionOverwriteAsync((SocketGuildUser)cmd.Data.Options.First().Value, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow));

                    await cmd.RespondAsync($"{cmd.User.Mention} à ajouté {((SocketGuildUser)cmd.Data.Options.First().Value).Mention} au ticket");
                }
                else
                {
                    await cmd.RespondAsync("Vous n'êtes pas un membre du staff", ephemeral: true);
                }
            }
            else
            {
                await cmd.RespondAsync("Vous n'êtes pas dans le bon salon", ephemeral: true);
            }
        }
        public static async Task RemovePlayer(SocketSlashCommand cmd)
        {
            var c = await cmd.GetChannelAsync();
            if (c.Name.Contains("ticket") || c.Name.Contains("traitement") || c.Name.Contains("logs"))
            {
                var staffRole = Bot.guild.GetRole(942507484982157402);

                if (((SocketGuildUser)cmd.User).Roles.Contains(staffRole))
                {
                    await Bot.guild.GetTextChannel(c.Id).AddPermissionOverwriteAsync((SocketGuildUser)cmd.Data.Options.First().Value, new OverwritePermissions(viewChannel: PermValue.Deny, sendMessages: PermValue.Deny, readMessageHistory: PermValue.Deny));

                    await cmd.RespondAsync($"{cmd.User.Mention} à retiré {((SocketGuildUser)cmd.Data.Options.First().Value).Mention} du ticket");
                }
                else
                {
                    await cmd.RespondAsync("Vous n'êtes pas un membre du staff", ephemeral: true);
                }
            }
            else
            {
                await cmd.RespondAsync("Vous n'êtes  pas dans le bon salon");
            }
        }

        public static async Task OnCommand(SocketSlashCommand cmd)
        {
            var mb = new ModalBuilder()
                .WithTitle("Création de ticket")
                .WithCustomId("ticket_creation")
                .AddTextInput("Nom IG", "ticket_ig_name", placeholder: "Optionel...", required: false)
                .AddTextInput("Type de ticket", "ticket_type", placeholder: "Remboursement/Boutique/Bug/Partenariat/Autres...", maxLength: 13, required: true)
                .AddTextInput("Quelle est le problème ?", "ticket_issue", placeholder: "Impossible de...", required: true)
                .AddTextInput("Autres informations ?", "ticket_other_info", placeholder: "D'autres choses que nous devons savoir...", required: false)
                .Build;

            try
            {
                await cmd.RespondWithModalAsync(mb.Invoke());
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static async Task OnModal(SocketModal modal)
        {
            try
            {
                await modal.DeferAsync();

                List<SocketMessageComponentData> components =
                modal.Data.Components.ToList();
                string nom_ig = components
                    .First(x => x.CustomId == "ticket_ig_name").Value;
                string type = components
                    .First(x => x.CustomId == "ticket_type").Value;
                string issue = components
                    .First(x => x.CustomId == "ticket_issue").Value;
                string other = components
                    .First(x => x.CustomId == "ticket_other_info").Value;

                if (nom_ig == "") { nom_ig = "Aucun"; }
                if (other == "") { other = "Aucun"; }

                var t = await GetId();
                var embed = new EmbedBuilder()
                    .WithTitle($"Information du ticket de {modal.User.Username} ID: {t}")
                    .AddField("Nom IG", nom_ig, true)
                    .AddField("Type", type, true)
                    .AddField("Problème", issue, false)
                    .AddField("Autre informations", other, false)
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .Build();

                var channel = await Bot.guild.CreateTextChannelAsync($"ticket-{type}-{t}");
                await channel.ModifyAsync(prop => prop.CategoryId = 959471170724446229);
                await channel.SyncPermissionsAsync();

                await channel.AddPermissionOverwriteAsync(modal.User, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow));

                tickets.Add(new TicketData
                {
                    userID = modal.User.Id,
                    ticketID = t,
                    NameIG = nom_ig,
                    ticketType = type,
                    ticketIssue = issue,
                    ticketOtherInfo = other,
                    ticketStatus = Status.pending,
                    ticketDate = DateTime.UtcNow.AddHours(2)
                });

                await SaveData();

                await channel.SendMessageAsync(embed: embed);
                await channel.SendMessageAsync($"<@{modal.User.Id}>\nUn membre du  vous repondras bientot, veuillez patitenter");
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
