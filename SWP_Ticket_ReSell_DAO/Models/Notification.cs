﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class Notification
{
    public int ID_Notification { get; set; }

    public string Title { get; set; }

    public string Event { get; set; }

    public DateTime? Organizing_time { get; set; }

    public int? ID_Ticket { get; set; }

    public int? ID_Order { get; set; }

    public int? ID_Request { get; set; }

    public int? ID_Customer { get; set; }

    public DateTime? Time_create { get; set; }
    public virtual Order ID_OrderNavigation { get; set; }

    public virtual Request ID_RequestNavigation { get; set; }

    public virtual Ticket ID_TicketNavigation { get; set; }
}