using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Feedback;
using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using SWP_Ticket_ReSell_DAO.DTO.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Order
{
    public class OrderNavigationDTO
    {
        public int ID_Order { get; set; }

        public string Payment_method { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }

        public DateTime? Shipping_time { get; set; }

        public DateTime? Create_At { get; set; }

        public virtual ICollection<OrderDetailDTO> OrderDetails { get; set; }

        public virtual ICollection<FeedbackReponseDTO> Feedbacks { get; set; } 

        public virtual ICollection<ReportResponseDTO> Reports { get; set; } 

    }
}
