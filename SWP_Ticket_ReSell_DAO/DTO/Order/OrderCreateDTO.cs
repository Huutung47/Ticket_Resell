using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWP_Ticket_ReSell_DAO;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;

namespace SWP_Ticket_ReSell_DAO.DTO.Order
{
    public class OrderCreateDTO
    {
        public int? ID_Customer { get; set; }

        public string Payment_method { get; set; }

        public List<CreateTicketOrderDTO> ticketIds { get; set; }

    }
}
