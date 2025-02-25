using ClassLibrary1;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentIntentController(IPaymentIntentService paymentIntentService) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                var id = Guid.NewGuid();
                await paymentIntentService.CreateAsync(id, request.Amount, request.Currency);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/fail")]
        public async Task<IActionResult> FailPaymentIntent(Guid id, [FromBody] string reason)
        {
            try
            {
                await paymentIntentService.FailAsync(id, reason);
                return Ok($"Intent {id} is now in failed status becouse of: {reason}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/finish")]
        public async Task<IActionResult> FinishPaymentIntent(Guid id)
        {
            try
            {
                await paymentIntentService.FinishAsync(id);
                return Ok($"Intent {id} is now in finished status");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("{id}/pending")]
        public async Task<IActionResult> PendingPaymentIntent(Guid id)
        {
            try
            {
                await paymentIntentService.PendingAsync(id);
                return Ok($"Intent {id} is now in pending status");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentIntent(Guid id)
        {
            try
            {
                var intent = await paymentIntentService.GetByIdAsync(id);
                return Ok(intent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CreatePaymentIntentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
