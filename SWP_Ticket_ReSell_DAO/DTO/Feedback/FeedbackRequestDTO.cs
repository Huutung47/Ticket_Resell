﻿using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Package
{
    public class FeedbackRequestDTO
    {
        public int? ID_Order { get; set; }

        public string Comment { get; set; }

        public int? Stars { get; set; }
        
    }
}
