using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Report;
using OfficeOpenXml;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.Package;

[ApiController]
[Route("api/[controller]")]
public class ExcelController : ControllerBase
{
    private readonly ServiceBase<Report> _serviceReport;
    private readonly ServiceBase<Customer> _serviceCustomer;
    private readonly ServiceBase<Order> _serviceOrder;
    private readonly ServiceBase<Package> _servicePackage;
    private readonly ServiceBase<Ticket> _serviceTicket;
    public ExcelController(ServiceBase<Report> serviceReport, ServiceBase<Customer> serviceCustomer, 
        ServiceBase<Order> serviceOrder, ServiceBase<Package> servicePackage, ServiceBase<Ticket> serviceTicket)
    {
        _serviceReport = serviceReport;
        _serviceCustomer = serviceCustomer;
        _serviceOrder = serviceOrder;
        _servicePackage = servicePackage;
        _serviceTicket = serviceTicket;
    }
    [HttpGet("excel-report")]
    public async Task<IActionResult> ExportExcelReport()
    {
        var data = await _serviceReport.FindListAsync<ReportPrintExcel>();
        var dataList = data.ToList();
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var sheet = package.Workbook.Worksheets.Add("Report");
            //do du lieu vao sheet 
            sheet.Cells.LoadFromCollection(dataList, true);
            package.Save();
        }
        stream.Position = 0;
        var fileName = $"Report {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",fileName);
    }

    [HttpGet("excel-user")]
    public async Task<IActionResult> ExportExcelUser()
    {
        var data = await _serviceCustomer.FindListAsync<CustomerPrintExcel>();
        var dataList = data.ToList();
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var sheet = package.Workbook.Worksheets.Add("User");
            //do du lieu vao sheet 
            sheet.Cells.LoadFromCollection(dataList, true);
            package.Save();
        }
        stream.Position = 0;
        var fileName = $"User {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("excel-order")]
    public async Task<IActionResult> ExportExcelOrder()
    {
        var data = await _serviceOrder.FindListAsync<OrderPrintExcel>();
        var dataList = data.ToList();
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var sheet = package.Workbook.Worksheets.Add("Order");
            //do du lieu vao sheet 
            sheet.Cells.LoadFromCollection(dataList, true);
            package.Save();
        }
        stream.Position = 0;
        var fileName = $"Order {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("excel-package")]
    public async Task<IActionResult> ExportExcelPackage()
    {
        var data = await _servicePackage.FindListAsync<PackagePrintExcel>();
        var dataList = data.ToList();
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var sheet = package.Workbook.Worksheets.Add("Package");
            //do du lieu vao sheet 
            sheet.Cells.LoadFromCollection(dataList, true);
            package.Save();
        }
        stream.Position = 0;
        var fileName = $"Package {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("excel-ticket")]
    public async Task<IActionResult> ExportExcelTicket()
    {
        var data = await _serviceTicket.FindListAsync<TicketPrintExcel>();
        var dataList = data.ToList();
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var sheet = package.Workbook.Worksheets.Add("Ticket");
            //do du lieu vao sheet 
            sheet.Cells.LoadFromCollection(dataList, true);
            package.Save();
        }
        stream.Position = 0;
        var fileName = $"Ticket {DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
