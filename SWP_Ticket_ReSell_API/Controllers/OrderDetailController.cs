using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : Controller
    {
        private readonly ServiceBase<OrderDetail> _service;

        public OrderDetailController(ServiceBase<OrderDetail> service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<IList<OrderDetailResponseDTO>>> GetOrderDetail()
        {
            var entities = await _service.FindListAsync<OrderDetailResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailResponseDTO>> GetOrderDetail(string id)
        {
            var entity = await _service.FindByAsync(p => p.ID_OrderDetail.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Order id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<OrderDetailResponseDTO>());
        }

        [HttpPut]
        public async Task<IActionResult> PutOrder(OrderDetailResponseDTO orderDetailRequest)
        {
            var entity = await _service.FindByAsync(p => p.ID_OrderDetail == orderDetailRequest.ID_OrderDetail);
            if (entity == null)
            {
                return Problem(detail: $"OrderDetail_ID {orderDetailRequest.ID_OrderDetail} cannot found", statusCode: 404);
            }
            orderDetailRequest.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update Order successfull.");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var orderDetail = await _service.FindByAsync(p => p.ID_OrderDetail == id);
            if (orderDetail == null)
            {
                return Problem(detail: $"Order_id {id} cannot found", statusCode: 404);
            }

            await _service.DeleteAsync(orderDetail);
            return Ok("Delete order successfull.");
        }

        [HttpGet("all-order-sellerId")]
        public async Task<ActionResult<IList<OrderResponseDTO>>> GetOrdersBySellerId(int sellerId)
        {
            var entities = await _service.FindListAsync<OrderResponseDTO>(t => t.ID_TicketNavigation.ID_Customer == sellerId);
            return Ok(entities);
        }
    }
}
