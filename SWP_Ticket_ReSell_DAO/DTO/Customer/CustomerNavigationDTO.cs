using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Customer
{
    public class CustomerNavigationDTO
    {
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public decimal? Average_feedback { get; set; }

        public virtual PackageRequestDTO? ID_PackageNavigation { get; set; }

        //public virtual RoleDTO? ID_RoleNavigation { get; set; }
    }
}
