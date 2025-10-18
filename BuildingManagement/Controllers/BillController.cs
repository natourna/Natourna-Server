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

        [HttpGet]
        public async Task<ActionResult<List<BillEntity>>> GetAllBills()
        {
            var bills = await _billManager.GetAllBillsAsync();

            return Ok(bills);
        }

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

        [HttpGet("compound/{compoundId}")]
        public async Task<ActionResult<List<BillEntity>>> GetBillsByCompoundId(int compoundId)
        {
            var bills = await _billManager.GetBillsByCompoundIdAsync(compoundId);

            return Ok(bills);
        }

        [HttpPost]
        public async Task<ActionResult<BillEntity>> CreateBill(BillEntity bill)
        {
            var createdBill = await _billManager.CreateBillAsync(bill);

            return CreatedAtAction(nameof(GetBillById), new { id = createdBill.Id }, createdBill);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BillEntity>> UpdateBill(int id, BillEntity bill)
        {
            var updatedBill = await _billManager.UpdateBillAsync(id, bill);

            if (updatedBill == null)
            {
                return NotFound();
            }

            return Ok(updatedBill);
        }

        [HttpDelete("{id}")]
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
        /// Mark a bill as paid. This will deduct the bill amount from the associated balance.
        /// </summary>
        /// <param name="id">The ID of the bill to mark as paid</param>
        /// <returns>The updated bill entity</returns>
        [HttpPost("{id}/mark-as-paid")]
        public async Task<ActionResult<BillEntity>> MarkBillAsPaid(int id)
        {
            var updatedBill = await _billManager.MarkBillAsPaidAsync(id);

            return Ok(updatedBill);
        }

        /// <summary>
        /// Mark a bill as unpaid. This will add the bill amount back to the associated balance.
        /// Use this to correct mistakes when a bill was accidentally marked as paid.
        /// </summary>
        /// <param name="id">The ID of the bill to mark as unpaid</param>
        /// <returns>The updated bill entity</returns>
        [HttpPost("{id}/mark-as-unpaid")]
        public async Task<ActionResult<BillEntity>> MarkBillAsUnpaid(int id)
        {
            var updatedBill = await _billManager.MarkBillAsUnpaidAsync(id);

            return Ok(updatedBill);
        }
    }
}