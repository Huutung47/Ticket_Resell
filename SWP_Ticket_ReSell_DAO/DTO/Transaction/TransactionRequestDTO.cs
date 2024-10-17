﻿using Microsoft.IdentityModel.Tokens;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP_Ticket_ReSell_DAO.DTO.Transaction
{
    public class TransactionRequestDTO
    {
        public int? ID_Order { get; set; }

        public int? ID_Customer { get; set; }

        public int? ID_Payment { get; set; }

        //public DateTime? Created_At { get; set; }
        //public string Status { get; set; }

        public double FinalPrice { get; set; }
    }
}