using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendAPI.Data;
using BackendAPI.Models;

namespace BackendAPI.Controllers{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees.ToListAsync();
            return Ok(employees);
        }


        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = employee.Id }, employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("generate-url")]
        public async Task<IActionResult> CreateURL([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model State Invalid:");
                foreach (var key in ModelState.Keys)
                    Console.WriteLine($"{key} = {ModelState[key]?.Errors.FirstOrDefault()?.ErrorMessage}");
                return BadRequest(ModelState);
            }
            employee.ProfileUrl = $"https://localhost:3000/view-employee/{employee.Id}";
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(new { id = employee.Id });
        }
        
        [HttpPost("upload-image/{id}")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            employee.ProfileImage = $"/images/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = employee.ProfileImage });
        }


    }
}