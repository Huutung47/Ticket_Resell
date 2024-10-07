using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Notificate
{
    public class NotificateDTO
    {
        public int ID_Notification { get; set; }

        public string Title { get; set; }

        public string Event { get; set; }

        public DateTime? Organization_day { get; set; }

        public TimeSpan? Organizing_time { get; set; }

        public int? ID_Ticket { get; set; }

    }
}
