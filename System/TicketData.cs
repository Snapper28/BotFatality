using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotFatality.System
{

    public enum Status
    {
        pending,
        taken,
        closed
    }

    [Serializable]
    public class TicketData
    {
        public ulong userID = 0;

        public int ticketID;

        public string? NameIG;
        public string ticketType;
        public string ticketIssue;
        public string? ticketOtherInfo;

        public Status ticketStatus;

        public ulong staffID = 0;

        public DateTime ticketDate;
    }
}
