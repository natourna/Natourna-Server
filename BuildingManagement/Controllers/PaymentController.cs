using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentApiManager _paymentManager;

        public PaymentController(IPaymentApiManager paymentManager)
        {
            _paymentManager = paymentManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<PaymentEntity>>> GetAllPayments()
        {
            var payments = await _paymentManager.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentEntity>> GetPaymentById(int id)
        {
            var payment = await _paymentManager.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpGet("bill/{billId}")]
        public async Task<ActionResult<List<PaymentEntity>>> GetPaymentsByBillId(int billId)
        {
            var payments = await _paymentManager.GetPaymentsByBillIdAsync(billId);
            return Ok(payments);
        }

        [HttpGet("apartment/{apartmentId}")]
        public async Task<ActionResult<List<PaymentEntity>>> GetPaymentsByApartmentId(int apartmentId)
        {
            var payments = await _paymentManager.GetPaymentsByApartmentIdAsync(apartmentId);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentEntity>> CreatePayment(PaymentEntity payment)
        {
            var createdPayment = await _paymentManager.CreatePaymentAsync(payment);
            return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentEntity>> UpdatePayment(int id, PaymentEntity payment)
        {
            var updatedPayment = await _paymentManager.UpdatePaymentAsync(id, payment);
            if (updatedPayment == null)
                return NotFound();

            return Ok(updatedPayment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var result = await _paymentManager.DeletePaymentAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}