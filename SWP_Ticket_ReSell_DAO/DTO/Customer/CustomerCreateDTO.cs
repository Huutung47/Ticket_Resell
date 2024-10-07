using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Customer
{
    public class CustomerCreateDTO
    {
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Tên không được chứa ký tự đặc biệt hoặc số.")]
        public string? Name { get; set; }


        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số liên lạc chỉ được chứa số và không được chứa ký tự đặc biệt hoặc chữ.")]
        public string? Contact { get; set; }


        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải có định dạng @gmail.com.")]
        public string? Email { get; set; }


        //[Range(0, 100, ErrorMessage = "AverageFeedback >= 0 and <= 100")]
        public decimal? Average_feedback { get; set; }


        //[RegularExpression(@"^\d{2}-\d{2}-\d{4}$", ErrorMessage = "Ngày phải có định dạng dd-MM-yyyy.")]
        public DateTime? PackageExpirationDate { get; set; }

        public int? ID_Role { get; set; }

        public int? ID_Package { get; set; }

        public string? Password { get; set; } // mã hóa từ FE sang base64 hoặc gì gì đó rồi lưu vào DB 

    }
}
