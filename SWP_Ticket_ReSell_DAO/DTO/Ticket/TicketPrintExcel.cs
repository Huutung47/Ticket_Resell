﻿using Microsoft.AspNetCore.Http;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Role;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Ticket
{
    public class TicketPrintExcel
    {
        public int ID_Ticket { get; set; }

        public int? ID_Customer { get; set; }

        public decimal? Price { get; set; }

        public string Ticket_category { get; set; }

        public bool? Ticket_type { get; set; }

        public int? Quantity { get; set; }

        public DateTime? Ticket_History { get; set; }

        public string Status { get; set; }

        public DateTime? Event_Date { get; set; }

        public string Show_Name { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public string Seat { get; set; }

        public int? Ticketsold { get; set; }

        public string Image { get; set; }





    }
}   
