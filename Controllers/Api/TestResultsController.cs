using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Controllers.Api
{
    [Route("api/test-results")]
    [ApiController]
    public class TestResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/test-results
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetAll()
        {
            return await _context.TestResults
                .Include(t => t.Patient)
                .Include(t => t.Doctor)
                .ToListAsync();
        }

        // GET: api/test-results/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestResult>> GetById(int id)
        {
            var testResult = await _context.TestResults
                .Include(t => t.Patient)
                .Include(t => t.Doctor)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (testResult == null)
            {
                return NotFound();
            }

            return testResult;
        }

        // POST: api/test-results
        [HttpPost]
        public async Task<ActionResult<TestResult>> Create(TestResult testResult)
        {
            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = testResult.Id }, testResult);
        }

        // PUT: api/test-results/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TestResult testResult)
        {
            if (id != testResult.Id)
            {
                return BadRequest();
            }

            _context.Entry(testResult).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TestResults.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/test-results/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null)
            {
                return NotFound();
            }

            _context.TestResults.Remove(testResult);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
