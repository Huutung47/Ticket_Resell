﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class Report
{
    public int ID_Report { get; set; }

    public string Comment { get; set; }

    public DateTime? History { get; set; }

    public int? ID_Order { get; set; }

    public virtual Order ID_OrderNavigation { get; set; }
}