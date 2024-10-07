using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Report
{
    public class ReportRequestDTO
    {
        public int ID_Customer { get; set; }

        public int ID_Order { get; set; }

        public string Comment { get; set; }

    }
}
