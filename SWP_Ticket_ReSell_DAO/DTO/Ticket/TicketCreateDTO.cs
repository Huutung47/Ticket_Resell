using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Ticket
{
    public class TicketCreateDTO
    {
        public int ID_Customer { get; set; }

        [RegularExpression(@"^[0-9]+(\.\d{1,2})?$", ErrorMessage = "Giá vé chỉ được chứa số và phần thập phân.")]
        public decimal? Price { get; set; }

        public string Ticket_category { get; set; }

        public bool? Ticket_type { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số lượng chỉ được chứa số.")]
        public int? Quantity { get; set; }
        public DateTime? Event_Date { get; set; }

        public string Show_Name { get; set; }

        public string Description { get; set; }
    }
}
