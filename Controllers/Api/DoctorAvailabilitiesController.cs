using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Controllers.Api
{
    [Route("api/doctor-availabilities")]
    [ApiController]
    public class DoctorAvailabilitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorAvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/doctor-availabilities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorAvailability>>> GetAll()
        {
            return await _context.DoctorAvailabilities
                .Include(d => d.Doctor)
                .ToListAsync();
        }

        // GET: api/doctor-availabilities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorAvailability>> GetById(int id)
        {
            var doctorAvailability = await _context.DoctorAvailabilities
                .Include(d => d.Doctor)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctorAvailability == null)
            {
                return NotFound();
            }

            return doctorAvailability;
        }

        // POST: api/doctor-availabilities
        [HttpPost]
        public async Task<ActionResult<DoctorAvailability>> Create(DoctorAvailability doctorAvailability)
        {
            _context.DoctorAvailabilities.Add(doctorAvailability);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = doctorAvailability.Id }, doctorAvailability);
        }

        // PUT: api/doctor-availabilities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DoctorAvailability doctorAvailability)
        {
            if (id != doctorAvailability.Id)
            {
                return BadRequest();
            }

            _context.Entry(doctorAvailability).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.DoctorAvailabilities.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/doctor-availabilities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctorAvailability = await _context.DoctorAvailabilities.FindAsync(id);
            if (doctorAvailability == null)
            {
                return NotFound();
            }

            _context.DoctorAvailabilities.Remove(doctorAvailability);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
