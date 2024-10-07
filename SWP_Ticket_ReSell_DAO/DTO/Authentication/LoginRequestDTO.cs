using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Authentication
{
    public class LoginRequestDTO 
    {
        public string? Email { get; set; }
        public string? Password { get; set; } 
    }
}
