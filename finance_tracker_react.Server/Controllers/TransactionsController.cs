using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using finance_tracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Globalization;

namespace finance_tracker.Controllers
{
    [ApiController]
    [Route("transactions/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly transactionsContext _context;

        public TransactionsController(transactionsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<transactions>>> GetAll()
        {
            return await _context.transactions.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<transactions>> Get(int id)
        {
            var item = await _context.transactions.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<transactions>> Create(transactions transaction)
        {
            _context.transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = transaction.id }, transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, transactions transaction)
        {
            if (id != transaction.id) return BadRequest();
            _context.Entry(transaction).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (! _context.transactions.Any(e => e.id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.transactions.FindAsync(id);
            if (item == null) return NotFound();
            _context.transactions.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
