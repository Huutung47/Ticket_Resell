using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.Payment;
using SWP_Ticket_ReSell_DAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Transaction
{
    public class TransactionResponseDTO
    {
        public int ID_Transaction { get; set; }
        public string Status { get; set; }

        public DateTime? Created_At { get; set; }

        public virtual CustomerResponseDTO ID_CustomerNavigation { get; set; }

        public virtual OrderNavigationDTO ID_OrderNavigation { get; set; }

        public virtual PaymentDTO ID_PaymentNavigation { get; set; }
    }
}
