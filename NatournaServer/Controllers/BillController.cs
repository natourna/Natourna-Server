using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
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
        public async Task<ActionResult<List<BillResponse>>> GetAllBills()
        {
            List<BillResponse> bills = await _billManager.GetAllBillsAsync();
            return Ok(bills);
        }

        /// <summary>
        /// Get bill by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BillResponse>> GetBillById(int id)
        {
            BillResponse? bill = await _billManager.GetBillByIdAsync(id);

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
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> CreateBill(BillRequest bill)
        {
            BillResponse createdBill = await _billManager.CreateBillAsync(bill);
            return CreatedAtAction(nameof(GetBillById), new { id = createdBill.Id }, createdBill);
        }

        /// <summary>
        /// Update bill - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> UpdateBill(int id, BillUpdateRequest bill)
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
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> DeleteBill(int id)
        {
            bool result = await _billManager.DeleteBillAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Mark bill as paid - Admin only
        /// </summary>
        [HttpPatch("{id}/mark-as-paid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> MarkBillAsPaid(int id)
        {
            BillResponse updatedBill = await _billManager.MarkBillAsPaidAsync(id);
            return Ok(updatedBill);
        }

        /// <summary>
        /// Mark bill as unpaid - Admin only
        /// </summary>
        [HttpPatch("{id}/mark-as-unpaid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> MarkBillAsUnpaid(int id)
        {
            BillResponse updatedBill = await _billManager.MarkBillAsUnpaidAsync(id);
            return Ok(updatedBill);
        }
    }
}