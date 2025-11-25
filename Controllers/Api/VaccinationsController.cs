using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VaccinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/vaccinations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vaccination>>> GetAll()
        {
            return await _context.Vaccinations
                .Include(v => v.Patient)
                .Include(v => v.Doctor)
                .ToListAsync();
        }

        // GET: api/vaccinations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vaccination>> GetById(int id)
        {
            var vaccination = await _context.Vaccinations
                .Include(v => v.Patient)
                .Include(v => v.Doctor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vaccination == null)
            {
                return NotFound();
            }

            return vaccination;
        }

        // POST: api/vaccinations
        [HttpPost]
        public async Task<ActionResult<Vaccination>> Create(Vaccination vaccination)
        {
            _context.Vaccinations.Add(vaccination);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
        }

        // PUT: api/vaccinations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Vaccination vaccination)
        {
            if (id != vaccination.Id)
            {
                return BadRequest();
            }

            _context.Entry(vaccination).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Vaccinations.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/vaccinations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);
            if (vaccination == null)
            {
                return NotFound();
            }

            _context.Vaccinations.Remove(vaccination);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
