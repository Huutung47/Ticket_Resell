using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Order
{
    public class OrderPackageCreateDTO
    {
        [Required(ErrorMessage = "ID_Customer không được để trống")]
        public int ID_Customer { get; set; }

        public string Payment_method { get; set; }

        [Required(ErrorMessage = "ID_Package không được để trống")]
        public int ID_Package { get; set; }
    }
}
