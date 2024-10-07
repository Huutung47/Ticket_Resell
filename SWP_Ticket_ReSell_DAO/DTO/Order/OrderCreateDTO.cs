using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Order
{
    public class OrderCreateDTO
    {
        public int ID_Order { get; set; }

        public int? ID_Ticket { get; set; }

        public string Payment_method { get; set; }

        public string Seller { get; set; }

        public string Buyer { get; set; }

        public decimal? Price { get; set; }

        public int? Quantity { get; set; }

        public string Status { get; set; }

        public DateTime? Shipping_time { get; set; }

        public DateTime? Order_time { get; set; }

    }
}
