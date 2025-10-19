using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Api.Requests.Payment;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
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
        public async Task<ActionResult<List<PaymentEntity>>> GetAllPayments()
        {
            var payments = await _paymentApiManager.GetAllPaymentsAsync();
            return Ok(payments);
        }

        /// <summary>
        /// Get payment by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentEntity>> GetPaymentById(int id)
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
        public async Task<ActionResult<List<PaymentEntity>>> GetPaymentsByApartmentId(int apartmentId)
        {
            var payments = await _paymentApiManager.GetPaymentsByApartmentIdAsync(apartmentId);
            return Ok(payments);
        }

        /// <summary>
        /// Create payment - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentEntity>> CreatePayment([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPayment = await _paymentApiManager.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
        }

        /// <summary>
        /// Update payment - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentEntity>> UpdatePayment(int id, PaymentEntity payment)
        {
            var updatedPayment = await _paymentApiManager.UpdatePaymentAsync(id, payment);
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
            var result = await _paymentApiManager.DeletePaymentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Mark payment as paid - Admin only
        /// </summary>
        [HttpPost("{id}/mark-as-paid")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentEntity>> MarkPaymentAsPaid(int id)
        {
            var updatedPayment = await _paymentApiManager.MarkPaymentAsPaidAsync(id);
            return Ok(updatedPayment);
        }

        /// <summary>
        /// Mark payment as unpaid - Admin only
        /// </summary>
        [HttpPost("{id}/mark-as-unpaid")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentEntity>> MarkPaymentAsUnpaid(int id)
        {
            var updatedPayment = await _paymentApiManager.MarkPaymentAsUnpaidAsync(id);
            return Ok(updatedPayment);
        }
    }
}