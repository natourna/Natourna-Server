using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillController : ControllerBase
    {
        private readonly IBillApiManager _billManager;

        public BillController(IBillApiManager billManager)
        {
            _billManager = billManager;
        }

        /// <summary>
        /// Get all bills - Any authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BillEntity>>> GetAllBills()
        {
            var bills = await _billManager.GetAllBillsAsync();
            return Ok(bills);
        }

        /// <summary>
        /// Get bill by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BillEntity>> GetBillById(int id)
        {
            var bill = await _billManager.GetBillByIdAsync(id);

            if (bill == null)
            {
                return NotFound();
            }

            return Ok(bill);
        }

        /// <summary>
        /// Create bill - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BillEntity>> CreateBill(BillEntity bill)
        {
            var createdBill = await _billManager.CreateBillAsync(bill);
            return CreatedAtAction(nameof(GetBillById), new { id = createdBill.Id }, createdBill);
        }

        /// <summary>
        /// Update bill - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BillEntity>> UpdateBill(int id, BillEntity bill)
        {
            var updatedBill = await _billManager.UpdateBillAsync(id, bill);

            if (updatedBill == null)
            {
                return NotFound();
            }

            return Ok(updatedBill);
        }

        /// <summary>
        /// Delete bill - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteBill(int id)
        {
            var result = await _billManager.DeleteBillAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Mark bill as paid - Admin only
        /// </summary>
        [HttpPost("{id}/mark-as-paid")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BillEntity>> MarkBillAsPaid(int id)
        {
            var updatedBill = await _billManager.MarkBillAsPaidAsync(id);
            return Ok(updatedBill);
        }

        /// <summary>
        /// Mark bill as unpaid - Admin only
        /// </summary>
        [HttpPost("{id}/mark-as-unpaid")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BillEntity>> MarkBillAsUnpaid(int id)
        {
            var updatedBill = await _billManager.MarkBillAsUnpaidAsync(id);
            return Ok(updatedBill);
        }
    }
}