using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Boxchat
{
    public class BoxchatDTO
    {
        public int ID_Boxchat { get; set; }

        public int? ID_Ticket { get; set; }

        public int? Seller_ID { get; set; }

        public int? Buyer_ID { get; set; }

        public string Chat_content { get; set; }
    }
}
