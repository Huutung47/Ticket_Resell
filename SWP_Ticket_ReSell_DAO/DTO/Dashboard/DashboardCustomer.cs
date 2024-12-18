﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Dashboard
{
    public class DashboardCustomer 
    {
        public int ID_Customer { get; set; }

        public string? Name { get; set; }

        public string? Password { get; set; }

        public string? Contact { get; set; }

        public string? Email { get; set; }

        public decimal? Average_feedback { get; set; }

        public int? ID_Role { get; set; }

        public int? ID_Package { get; set; }
        public int? Number_of_tickets_can_posted { get; set; }

        public string Avatar { get; set; }


    }
}
