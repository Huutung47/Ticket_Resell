using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Payment
{
    public class CreatePaymentResponse
    {
        public string? Message { get; set; }
        public string? Url { get; set; }
        public string DisplayType { get; set; }
    }
}
