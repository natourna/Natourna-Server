using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Requests.Payment;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentApiManager _paymentApiManager;

        public PaymentController(IPaymentApiManager paymentApiManager)
        {
            _paymentApiManager = paymentApiManager;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PaymentResponse>>> GetPayments([FromQuery] PagedQuery paging, [FromQuery] int? apartmentId, [FromQuery] bool? isPaid, [FromQuery] DateTime? dueBefore)
        {
            var payments = await _paymentApiManager.GetPaymentsAsync(paging.Page, paging.PageSize, apartmentId, isPaid, dueBefore);
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetPaymentById(int id)
        {
            var payment = await _paymentApiManager.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] PaymentRequest request)
        {
            PaymentResponse createdPayment = await _paymentApiManager.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> UpdatePayment(int id, [FromBody] PaymentUpdateRequest request)
        {
            PaymentResponse? updatedPayment = await _paymentApiManager.UpdatePaymentAsync(id, request);
            if (updatedPayment == null)
            {
                return NotFound();
            }

            return Ok(updatedPayment);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeletePayment(int id)
        {
            bool result = await _paymentApiManager.DeletePaymentAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{id}/mark-as-paid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> MarkPaymentAsPaid(int id)
        {
            PaymentResponse updatedPayment = await _paymentApiManager.MarkPaymentAsPaidAsync(id);
            return Ok(updatedPayment);
        }

        [HttpPatch("{id}/mark-as-unpaid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> MarkPaymentAsUnpaid(int id)
        {
            PaymentResponse updatedPayment = await _paymentApiManager.MarkPaymentAsUnpaidAsync(id);
            return Ok(updatedPayment);
        }
    }
}
