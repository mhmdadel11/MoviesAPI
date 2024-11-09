using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MovieAPI.Data;
using MovieAPI.Dtos;
using MovieAPI.Models;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //private new List<String> _allowedExtenstions = new List<string> { ".jpj", ".Png" };
        /* private long _maxAllowedPosterSize = 1048576;*/  //1048576 اللى هى 1MB
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()     //هيجيب كل الموفيز الموجوده فالداتا بيز
        {

            var movies = await _context.Movies    //هنا بيجيب اعلى جنرى اللى اى دى بتاعه الكبيره 
            .OrderByDescending(x => x.Rate)
            .Include(m => m.Genre)
            .Select(m => new MovieDetailsDto
            {
                Id = m.Id,
                GenreId = m.GenreId,
                GenreName = m.Genre.Name,
                Poster = m.Poster,
                Rate = m.Rate,
                StoreLine = m.StoreLine,
                Title = m.Title,
                Year = m.Year
            })
            .ToListAsync();
            return Ok(movies);
        }


        //نبعت ال هى دى بتاع موفى معين و هى ترجعلنا الداتا بتاع الموفى الااى دى بتاعه 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);
            if (movie == null)

                return NotFound();
            var dto = new MovieDetailsDto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre.Name,
                Poster = movie.Poster,
                Rate = movie.Rate,
                StoreLine = movie.StoreLine,
                Title = movie.Title,
                Year = movie.Year
            };

            return Ok(dto);
        }



        // تبعت الجنرى اى دى و هى تبعتلنا الموفيز بس اللى جنرى اى دى بتساوى الجنرى اى دى اللى بعتها ف بارامترز بتاعى
        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _context.Movies
            .Where(m => m.GenreId == genreId)
            .OrderByDescending(x => x.Rate)
            .Include(m => m.GenreId)
            .Select(m => new MovieDetailsDto
            {
                Id = m.Id,
                GenreId = m.GenreId,
                GenreName = m.Genre.Name,
                Poster = m.Poster,
                Rate = m.Rate,
                StoreLine = m.StoreLine,
                Title = m.Title,
                Year = m.Year
            })
            .ToListAsync();
            return Ok(movies);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MoviesDto dto)
        {
            //الباص الخاص بالصور
            //if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
            //    return BadRequest("Only .Png and .Jpg images are allowed");
            //if (dto.Poster.Length > _maxAllowedPosterSize)
            //    return BadRequest("Max allowed size for poster is 1MB");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("Invalid Genre Id");

            //اتعمل مع صورة البوستر اللى جايه مع الريكويست و احولها ل اراى اوف بايت و بعد كدا ابعتها للبوستر الموجود فى الموفى
            // jpeg, ...و لازم لما اتعامل مع فايل احدد حجم الفايل المبعوت  و لازم نحدد اكستنشن معينه تتقبل ف السيف 
            //هنا هحول ال iform file to array of byte

            using var dataStream = new MemoryStream();
            await dto.Poster.CopyToAsync(dataStream);

            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                Poster = dataStream.ToArray(),
                Rate = dto.Rate,
                StoreLine = dto.StoreLine,
                Year = dto.Year
            };
            await _context.AddAsync(movie);
            _context.SaveChanges();
            return Ok(movie);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id,[FromForm]MoviesDto dto)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound($"No Movies WasFound with id {id}");
          


            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("Invalid Genre Id");

            if (dto.Poster != null)
            {
                //if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                //    return BadRequest("Only .Png and .Jpg images are allowed");
                //if (dto.Poster.Length > _maxAllowedPosterSize)
                //    return BadRequest("Max allowed size for poster is 1MB");


                //اتعمل مع صورة البوستر اللى جايه مع الريكويست و احولها ل اراى اوف بايت و بعد كدا ابعتها للبوستر الموجود فى الموفى
                // jpeg, ...و لازم لما اتعامل مع فايل احدد حجم الفايل المبعوت  و لازم نحدد اكستنشن معينه تتقبل ف السيف 
                //هنا هحول ال iform file to array of byte

                using var dataStream = new MemoryStream();
                await dto.Poster.CopyToAsync(dataStream);
            }

            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.StoreLine = dto.StoreLine;
            movie.Rate = dto.Rate;

            _context.SaveChanges();
            return Ok(movie);


        }
        




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound("No Movie Was Found With Id {id}");

            _context.Remove(movie);
            _context.SaveChanges();

            return Ok(movie);
        }

    
    }
}
