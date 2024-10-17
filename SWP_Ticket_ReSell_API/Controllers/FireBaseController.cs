using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using Repository;

[ApiController]
[Route("api/[controller]")]
public class FirebaseController : ControllerBase
{
    private readonly FirebaseStorageService _firebaseStorageService;
    private readonly ServiceBase<Ticket> _serviceTicket;

    public FirebaseController(FirebaseStorageService firebaseStorageService, ServiceBase<Ticket> serviceTicket)
    {
        _firebaseStorageService = firebaseStorageService;
        _serviceTicket = serviceTicket;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadImage([FromForm] TicketImageDTO ticketRequest)
    {
        if (ticketRequest.Image != null && ticketRequest.Image.Length > 0)
        {
            // Tải ảnh lên Firebase
            using (var stream = ticketRequest.Image.OpenReadStream())
            {
                // Tải file lên Firebase và nhận URL của ảnh
                var imageUrl = await _firebaseStorageService.UploadFileAsync(stream, ticketRequest.Image.FileName);

                // Trả về URL của ảnh mà không lưu vào database
                return Ok(imageUrl);
            }
        }
        else
        {
            return BadRequest("Please add an image.");
        }
    }

}
