using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.Dtos;
using MovieAPI.Models;
using System.Reflection.Metadata.Ecma335;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public GenresController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _context.Genres.OrderBy(g=>g.Name).ToListAsync();  // هيرجع كل الداتا الموجوده
            return Ok(genres);
        }
        [HttpPost]
        public async Task<IActionResult>CreateAsyncTask(CreateGenreDto dto)
        {
            var genre = new Genre { Name = dto.Name };
            await _context.AddAsync(genre);
            _context.SaveChanges();
            return Ok(genre);

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id,[FromBody]CreateGenreDto dto)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == id);
            if (genre == null)
                return NotFound($"No Genre Was Found With Id:{id}");
            genre.Name = dto.Name;
            _context.SaveChanges();
            return Ok(genre);
           

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult>DeleteAsync(int id)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == id);
            if (genre == null)
                return NotFound($"No Genre WasFound With Id:{id}");
            _context.Remove(genre);
            _context.SaveChanges();

            return Ok(genre);
        }










    }
}
