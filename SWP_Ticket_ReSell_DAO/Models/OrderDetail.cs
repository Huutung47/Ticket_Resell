﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class OrderDetail
{
    public int ID_OrderDetail { get; set; }

    public int? ID_Order { get; set; }

    public int? ID_Ticket { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public decimal? Total_price { get; set; }

    public virtual Order ID_OrderNavigation { get; set; }

    public virtual Ticket ID_TicketNavigation { get; set; }
}