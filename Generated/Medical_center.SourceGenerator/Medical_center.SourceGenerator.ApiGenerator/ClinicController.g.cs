#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Medical_center.Models;
using Medical_center.Models.Generated;
using Medical_center.Data;

namespace Medical_center.Models.Generated.Controllers
{
    [ApiController]
    [Route("api/generated/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClinicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClinicDto>>> GetAll()
        {
            var items = await _context.Clinics
                .Select(x => x.ToDto())
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClinicDto>> GetById(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            return Ok(clinic.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<ClinicDto>> Create(ClinicDto dto)
        {
            var clinic = new Clinic
            {
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber
            };
            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClinicDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();

            clinic.Name = dto.Name;
            clinic.Address = dto.Address;
            clinic.PhoneNumber = dto.PhoneNumber;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
