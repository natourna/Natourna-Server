using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }
}