using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Ticket
{
    public class TicketRequestDTO
    {
        public int ID_Ticket { get; set; }

        //public int? ID_Customer { get; set; }

        public decimal Price { get; set; }

        //public string Ticket_category { get; set; }

        //public bool Ticket_type { get; set; }

        public int Quantity { get; set; }

        //public DateTime Ticket_History { get; set; }

        public string Status { get; set; }

        //public DateTime Event_Date { get; set; }

        //public string Show_Name { get; set; }

        //public string Location { get; set; }

        //public string Description { get; set; }

        //public int? Seat { get; set; }

        //public int? Ticketsold { get; set; }

        //public List<FormFile> Images { get; set; }

    }
}
