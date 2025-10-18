using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using BuildingManagement.Models.Requests.Payment;
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

        [HttpGet]
        public async Task<ActionResult<List<PaymentEntity>>> GetAllPayments()
        {
            var payments = await _paymentApiManager.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentEntity>> GetPaymentById(int id)
        {
            var payment = await _paymentApiManager.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpGet("apartment/{apartmentId}")]
        public async Task<ActionResult<List<PaymentEntity>>> GetPaymentsByApartmentId(int apartmentId)
        {
            var payments = await _paymentApiManager.GetPaymentsByApartmentIdAsync(apartmentId);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentEntity>> CreatePayment([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdPayment = await _paymentApiManager.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentEntity>> UpdatePayment(int id, PaymentEntity payment)
        {
            var updatedPayment = await _paymentApiManager.UpdatePaymentAsync(id, payment);
            if (updatedPayment == null)
                return NotFound();

            return Ok(updatedPayment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var result = await _paymentApiManager.DeletePaymentAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/mark-as-paid")]
        public async Task<ActionResult<PaymentEntity>> MarkPaymentAsPaid(int id)
        {
            var updatedPayment = await _paymentApiManager.MarkPaymentAsPaidAsync(id);
            return Ok(updatedPayment);
        }

        [HttpPost("{id}/mark-as-unpaid")]
        public async Task<ActionResult<PaymentEntity>> MarkPaymentAsUnpaid(int id)
        {
            var updatedPayment = await _paymentApiManager.MarkPaymentAsUnpaidAsync(id);
            return Ok(updatedPayment);
        }
    }
}