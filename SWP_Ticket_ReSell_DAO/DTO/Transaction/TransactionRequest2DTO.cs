using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Transaction
{
    public class TransactionRequest2DTO
    {
        public int? ID_Order { get; set; }
        public int? ID_Seller { get; set; }
        public int? ID_Package { get; set; }

        [Required(ErrorMessage = "ID_Customer không được để trống")]
        public int? ID_Customer { get; set; }

        [Required(ErrorMessage = "ID_Payment không được để trống")]
        public int? ID_Payment { get; set; }

        [RegularExpression(@"^(Ticket|Package)$", ErrorMessage = "Transaction_Type chỉ có thể là 'Ticket' hoặc 'Package'")]
        public string Transaction_Type { get; set; }

        public double FinalPrice { get; set; }
    }
}
