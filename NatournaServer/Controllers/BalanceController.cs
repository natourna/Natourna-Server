using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Api;
using NatournaServer.Models.Api.Requests.Balance;
using NatournaServer.Models.Api.Response.Balance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NatournaServer.Controllers
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

        [HttpGet]
        public async Task<ActionResult<List<BalanceResponse>>> GetAllBalances([FromQuery] int? compoundId)
        {
            var balances = await _balanceApiManager.GetAllBalancesAsync(compoundId);
            return Ok(balances);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BalanceResponse>> GetBalanceById(int id)
        {
            var balance = await _balanceApiManager.GetBalanceByIdAsync(id);
            if (balance == null)
            {
                return NotFound();
            }

            return Ok(balance);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BalanceResponse>> CreateBalance([FromBody] BalanceRequest request)
        {
            var createdBalance = await _balanceApiManager.CreateBalanceAsync(request);
            return CreatedAtAction(nameof(GetBalanceById), new { id = createdBalance.Id }, createdBalance);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BalanceResponse>> UpdateBalance(int id, [FromBody] BalanceRequest request)
        {
            var updatedBalance = await _balanceApiManager.UpdateBalanceAsync(id, request);
            if (updatedBalance == null)
            {
                return NotFound();
            }

            return Ok(updatedBalance);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
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
