using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Role;
using SWP_Ticket_ReSell_DAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Customer
{
    public class CustomerResponseDTO
    {
        public int ID_Customer { get; set; }

        public string? Name { get; set; }

        public string? Password { get; set; }

        public string? Contact { get; set; }

        public string? Email { get; set; }

        public decimal? Average_feedback { get; set; }

        public int? ID_Role { get; set; }

        public int? ID_Package { get; set; }

        public DateTime? PackageExpirationDate { get; set; }

        public virtual FeedbackRequestDTO? ID_PackageNavigation { get; set; }

        public virtual RoleDTO? ID_RoleNavigation { get; set; }
    }
}
