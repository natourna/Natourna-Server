using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Requests.Payment;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.Payment;
using NatournaServer.Models.Entities;
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

        /// <summary>
        /// Get all payments - Any authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<PaymentResponse>>> GetAllPayments([FromQuery] PagedQuery query, [FromQuery] int? cycleId = null, [FromQuery] bool? isPaid = null, [FromQuery] bool? overdue = null)
        {
            var payments = await _paymentApiManager.GetPagedPaymentsAsync(query, cycleId: cycleId, isPaid: isPaid, overdue: overdue);
            return Ok(payments);
        }

        /// <summary>
        /// Get payment by ID - Any authenticated user
        /// </summary>
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

        /// <summary>
        /// Get payments by apartment ID - Any authenticated user
        /// </summary>
        [HttpGet("apartment/{apartmentId}")]
        public async Task<ActionResult<PagedResponse<PaymentResponse>>> GetPaymentsByApartmentId(int apartmentId, [FromQuery] PagedQuery query)
        {
            var payments = await _paymentApiManager.GetPagedPaymentsAsync(query, apartmentId: apartmentId);
            return Ok(payments);
        }

        /// <summary>
        /// Create payment - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] PaymentRequest request)
        {
            PaymentResponse createdPayment = await _paymentApiManager.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
        }

        /// <summary>
        /// Update payment - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> UpdatePayment(int id, PaymentUpdateRequest payment)
        {
            PaymentResponse? updatedPayment = await _paymentApiManager.UpdatePaymentAsync(id, payment);
            if (updatedPayment == null)
            {
                return NotFound();
            }

            return Ok(updatedPayment);
        }

        /// <summary>
        /// Delete payment - Admin only
        /// </summary>
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

        /// <summary>
        /// Mark payment as paid - Admin only
        /// </summary>
        [HttpPatch("{id}/mark-as-paid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> MarkPaymentAsPaid(int id)
        {
            PaymentResponse updatedPayment = await _paymentApiManager.MarkPaymentAsPaidAsync(id);
            return Ok(updatedPayment);
        }

        /// <summary>
        /// Mark payment as unpaid - Admin only
        /// </summary>
        [HttpPatch("{id}/mark-as-unpaid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaymentResponse>> MarkPaymentAsUnpaid(int id)
        {
            PaymentResponse updatedPayment = await _paymentApiManager.MarkPaymentAsUnpaidAsync(id);
            return Ok(updatedPayment);
        }
    }
}