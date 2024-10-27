using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Order
{
    public class OrderDTO
    {
        public int ID_Order { get; set; }

        public int? ID_Customer { get; set; }

        public string Payment_method { get; set; }

        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }

        public DateTime? Shipping_time { get; set; }

        public DateTime? Create_At { get; set; }
    }
}
