using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Authentication
{
    public class AccessTokenResponse
    {
        public string TokenType { get; } = "Bearer";
        public int ID { get; set; }
        public string AccessToken { get; init; }
        public long ExpiresIn { get; init; }
    }
}
