﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class Request
{
    public int ID_Request { get; set; }

    public int ID_Ticket { get; set; }

    public int ID_Customer { get; set; } //người mua 

    public decimal? Price_want { get; set; }

    public DateTime? History { get; set; }

    public virtual Customer ID_CustomerNavigation { get; set; }

    public virtual Ticket ID_TicketNavigation { get; set; }
}