using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Ticket
{
    public class TickerFilterDTO
    {
        public List<string>? ticketCategory { get; set; }

        public List<string>? location { get; set; }
    }
}
