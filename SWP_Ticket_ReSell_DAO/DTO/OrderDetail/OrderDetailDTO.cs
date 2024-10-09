using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.OrderDetail
{
    public class OrderDetailDTO
    {
        public int ID_OrderDetail { get; set; }

        public int? ID_Order { get; set; }

        public int? ID_Ticket { get; set; }

        public decimal? Price { get; set; }

        public int? Quantity { get; set; }

    }
}
