using BuildingManagement.Interfaces.Api;
using BuildingManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BalanceController : ControllerBase
    {
        private readonly IBalanceApiManager _balanceApiManager;

        public BalanceController(IBalanceApiManager balanceApiManager)
        {
            _balanceApiManager = balanceApiManager;
        }

        /// <summary>
        /// Get all balances - Any authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BalanceEntity>>> GetAllBalances()
        {
            var balances = await _balanceApiManager.GetAllBalancesAsync();
            return Ok(balances);
        }

        /// <summary>
        /// Get balance by ID - Any authenticated user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BalanceEntity>> GetBalanceById(int id)
        {
            var balance = await _balanceApiManager.GetBalanceByIdAsync(id);
            if (balance == null)
            {
                return NotFound();
            }

            return Ok(balance);
        }

        /// <summary>
        /// Get balances by compound ID - Any authenticated user
        /// </summary>
        [HttpGet("compound/{compoundId}")]
        public async Task<ActionResult<List<BalanceEntity>>> GetBalancesByCompoundIdAsync(int compoundId)
        {
            var balances = await _balanceApiManager.GetBalancesByCompoundIdAsync(compoundId);
            return Ok(balances);
        }

        /// <summary>
        /// Create balance - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BalanceEntity>> CreateBalance(BalanceEntity balance)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBalance = await _balanceApiManager.CreateBalanceAsync(balance);
            return CreatedAtAction(nameof(GetBalanceById), new { id = createdBalance.Id }, createdBalance);
        }

        /// <summary>
        /// Update balance - Admin only
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BalanceEntity>> UpdateBalance(int id, BalanceEntity balance)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBalance = await _balanceApiManager.UpdateBalanceAsync(id, balance);
            if (updatedBalance == null)
            {
                return NotFound();
            }

            return Ok(updatedBalance);
        }

        /// <summary>
        /// Delete balance - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteBalance(int id)
        {
            var result = await _balanceApiManager.DeleteBalanceAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}