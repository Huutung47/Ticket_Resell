﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Ticket
{
    public class TicketUpdateCustomerDTO
    {
        public int ID_Ticket { get; set; }

        public int? ID_Customer { get; set; }
    }
}
