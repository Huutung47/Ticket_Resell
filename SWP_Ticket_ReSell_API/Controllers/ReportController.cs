using Azure;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Report;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ServiceBase<Report> _serviceReport;
        private readonly ServiceBase<Order> _serviceOrder;
        public ReportController(ServiceBase<Report> serviceReport, ServiceBase<Order> serviceOrder)
        {
            _serviceReport = serviceReport;
            _serviceOrder = serviceOrder;
        }

        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<IList<ReportResponseDTO[]>>> GetReport()
        {
            var entities = await _serviceReport.FindListAsync<ReportResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        //[Authorize]
        public async Task<ActionResult<ReportResponseDTO>> GetReport(string id)
        {
            var entity = await _serviceReport.FindByAsync(p => p.ID_Report.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Report id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<ReportResponseDTO>());
        }

        [HttpPut]
        //[Authorize]
        public async Task<IActionResult> PutReport(ReportResponseDTO ticketRequest)
        {
            var entity = await _serviceReport.FindByAsync(p => p.ID_Report == ticketRequest.ID_Report);
            if (entity == null)
            {
                return Problem(detail: $"Report_id {ticketRequest.ID_Report} cannot found", statusCode: 404);
            }
            ticketRequest.Adapt(entity);
            await _serviceReport.UpdateAsync(entity);
            return Ok("Update Report successfull.");
        }

        [HttpPost]
        //[Authorize]
        public async Task<ActionResult<TicketResponseDTO>> PostReport(ReportRequestDTO reportRequest)
        {
            var ticket = new Report()
            {
                History = DateTime.Now,
            };
            reportRequest.Adapt(ticket);
            await _serviceReport.CreateAsync(ticket);
            return Ok("Create Report successfull.");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var ticket = await _serviceReport.FindByAsync(p => p.ID_Report == id);
            if (ticket == null)
            {
                return Problem(detail: $"Report_id {id} cannot found", statusCode: 404);
            }

            await _serviceReport.DeleteAsync(ticket);
            return Ok("Delete Report successfull.");
        }

        [HttpGet("report-order-refund-total-price")]
        [Authorize]
        public async Task<ActionResult<decimal>> GetMoneyTicketReport(int orderid)
        {
            var reports = await _serviceReport.FindListAsync<Report>(r => r.ID_Order == orderid);
            if (reports == null || !reports.Any())
            {
                return Ok();
            }
            var orderIds = reports.Select(r => r.ID_Order).Distinct().ToList();
            decimal? totalPrice = 0;
            foreach (var id in orderIds)
            {
                var order = await _serviceOrder.FindByAsync(o => o.ID_Order == id);
                if (order != null)
                {
                    totalPrice += order.TotalPrice; 
                }
            }
            return Ok(totalPrice);
        }

    }
}
