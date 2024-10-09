using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OrderController : Controller
    {
        private readonly ServiceBase<Order> _service;

        public OrderController(ServiceBase<Order> service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<IList<OrderResponseDTO>>> GetOrder()
        {
            var entities = await _service.FindListAsync<OrderResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrder(string id)
        {
            var entity = await _service.FindByAsync(p => p.ID_Order.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Order id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<OrderResponseDTO>());
        }

        [HttpPut]
        public async Task<IActionResult> PutOrder(OrderResponseDTO orderRequest)
        {
            var entity = await _service.FindByAsync(p => p.ID_Order == orderRequest.ID_Order);
            if (entity == null)
            {
                return Problem(detail: $"Order_id {orderRequest.ID_Order} cannot found", statusCode: 404);
            }
            orderRequest.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update Order successfull.");
        }
        
        [HttpPost("")]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderRequest)
        {
            //Validation
            

            var order = new Order()
            {
                

                
            };
            orderRequest.Adapt(order);
            await _service.CreateAsync(order);
            return Ok("Create order successfull.\n" +
                $"ID_Order: {order.ID_Order}");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _service.FindByAsync(p => p.ID_Order == id);
            if (order == null)
            {
                return Problem(detail: $"Order_id {id} cannot found", statusCode: 404);
            }

            await _service.DeleteAsync(order);
            return Ok("Delete order successfull.");
        }
    }
}
