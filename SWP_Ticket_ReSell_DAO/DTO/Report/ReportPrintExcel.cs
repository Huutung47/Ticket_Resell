﻿using SWP_Ticket_ReSell_DAO.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Report
{
    public class ReportPrintExcel
    {
        public int ID_Report { get; set; }

        public int ID_Order { get; set; }

        public string Comment { get; set; }

        public DateTime? History { get; set; }
        //public virtual OrderResponseDTO ID_OrderNavigation { get; set; }
    }
}