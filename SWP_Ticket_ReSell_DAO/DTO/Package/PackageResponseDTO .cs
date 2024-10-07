using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Package
{
    public class PackageResponseDTO
    {
        public int ID_Package { get; set; }

        public string Name_Package { get; set; }

        public decimal? Price { get; set; }

        public int Time_package { get; set; }

        public int? Ticket_can_post { get; set; }

        public string Description { get; set; }

    }
}
