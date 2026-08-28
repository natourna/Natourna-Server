using NatrounaServer.Interfaces.Api;
using NatrounaServer.Models.Api.Requests.Payment;
using NatrounaServer.Models.Api.Response.Payment;
using NatrounaServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatrounaServer.Controllers
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
        public async Task<ActionResult<List<PaymentResponse>>> GetAllPayments()
        {
            var payments = await _paymentApiManager.GetAllPaymentsAsync();
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
        public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByApartmentId(int apartmentId)
        {
            List<PaymentResponse> payments = await _paymentApiManager.GetPaymentsByApartmentIdAsync(apartmentId);
            return Ok(payments);
        }

        /// <summary>
        /// Create payment - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PaymentResponse createdPayment = await _paymentApiManager.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
        }

        /// <summary>
        /// Update payment - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentResponse>> UpdatePayment(int id, PaymentEntity payment)
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentResponse>> MarkPaymentAsPaid(int id)
        {
            PaymentResponse updatedPayment = await _paymentApiManager.MarkPaymentAsPaidAsync(id);
            return Ok(updatedPayment);
        }

        /// <summary>
        /// Mark payment as unpaid - Admin only
        /// </summary>
        [HttpPatch("{id}/mark-as-unpaid")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentResponse>> MarkPaymentAsUnpaid(int id)
        {
            PaymentResponse updatedPayment = await _paymentApiManager.MarkPaymentAsUnpaidAsync(id);
            return Ok(updatedPayment);
        }
    }
}