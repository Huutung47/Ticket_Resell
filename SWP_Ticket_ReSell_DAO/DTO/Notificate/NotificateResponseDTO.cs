using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Request;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Notificate
{
    public class NotificateResponseDTO
    {
        public int ID_Notification { get; set; }

        public string? Title { get; set; }

        public string? Event { get; set; }

        public DateTime? Organizing_time { get; set; }

        public int ID_Ticket { get; set; }

        public int? ID_Order { get; set; }

        public int? ID_Request { get; set; }
        public int? ID_Customer { get; set; }
        public DateTime? Time_create { get; set; }

        public virtual RequestRequestDTO? ID_RequestNavigation { get; set; }

        public virtual OrderDTO? ID_OrderNavigation { get; set; }

        public virtual TicketResponseDTO? ID_TicketNavigation { get; set; }
    }
}
