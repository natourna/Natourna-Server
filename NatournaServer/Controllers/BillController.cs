using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Bill;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Bill;
using NatournaServer.Models.Api.Response.Paging;
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

        [HttpGet]
        public async Task<ActionResult<PagedResponse<BillResponse>>> GetBills([FromQuery] PagedQuery paging, [FromQuery] bool? isPaid)
        {
            var bills = await _billManager.GetBillsAsync(paging.Page, paging.PageSize, isPaid);
            return Ok(bills);
        }

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

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> CreateBill([FromBody] BillRequest request)
        {
            BillResponse createdBill = await _billManager.CreateBillAsync(request);
            return CreatedAtAction(nameof(GetBillById), new { id = createdBill.Id }, createdBill);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> UpdateBill(int id, [FromBody] BillUpdateRequest request)
        {
            var updatedBill = await _billManager.UpdateBillAsync(id, request);

            if (updatedBill == null)
            {
                return NotFound();
            }

            return Ok(updatedBill);
        }

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

        [HttpPatch("{id}/mark-as-paid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> MarkBillAsPaid(int id)
        {
            BillResponse updatedBill = await _billManager.MarkBillAsPaidAsync(id);
            return Ok(updatedBill);
        }

        [HttpPatch("{id}/mark-as-unpaid")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BillResponse>> MarkBillAsUnpaid(int id)
        {
            BillResponse updatedBill = await _billManager.MarkBillAsUnpaidAsync(id);
            return Ok(updatedBill);
        }
    }
}
