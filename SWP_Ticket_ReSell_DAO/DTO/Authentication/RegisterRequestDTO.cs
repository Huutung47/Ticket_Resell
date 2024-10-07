using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Authentication
{
    public class RegisterRequestDTO 
    {
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải có định dạng @gmail.com.")]
        public string Email { get; set; }

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Tên không được chứa ký tự đặc biệt hoặc số.")]
        public string? Name { get; set; }

        public string? Password { get; set; }

        public string ConfirmPassWord { get; set; }

    }
}
