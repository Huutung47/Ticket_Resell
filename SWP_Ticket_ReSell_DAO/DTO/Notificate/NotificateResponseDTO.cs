﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Notificate
{
    public class NotificateResponseDTO
    {
        public int ID_Notification { get; set; }

        public string Title { get; set; }

        public string Event { get; set; }

        public DateTime Organizing_time { get; set; }

        public int ID_Ticket { get; set; }

        public int? ID_Order { get; set; }

        public int? ID_Request { get; set; }

    }
}