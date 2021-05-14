using CinemaApi.Data;
using CinemaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private CinemaDbContext _dbContext;
        public MoviesController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("[action]")]
        public IActionResult AllMovies(string sort,int? pageNumber,int? pageSize)
        {
            var currentPageNumber = pageNumber ?? 1;
            var currentpageSize = pageSize ?? 5;
            var movies=from movie in _dbContext.Movies
            select new
            {
                Id = movie.Id,
                Name = movie.Name,
                Duration = movie.Duration,
                Language = movie.Language,
                Rating = movie.Rating,
                Genre = movie.Genre,
                ImageUrl = movie.ImageUrl
            };
            switch(sort)
            {
                case "desc":
                    return Ok(movies.OrderByDescending(m => m.Rating).Skip((currentPageNumber - 1) * currentpageSize).Take(currentpageSize));
                case "asc":
                    return Ok(movies.OrderBy(m => m.Rating).Skip((currentPageNumber - 1) * currentpageSize).Take(currentpageSize));
                default:
                    return Ok(movies.Skip((currentPageNumber - 1)* currentpageSize).Take(currentpageSize));
            }
        }

        //api/movies/MovieDetail/1
        [Authorize]
        [HttpGet("[action]/{id}")]
        public IActionResult MovieDetail(int id)
        {
            var movie=_dbContext.Movies.Find(id);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }

        [Authorize]
        [HttpGet("[action]")]
        //api/movies/findmovies?moviename=MissionImpossible
        public IActionResult FindMovies(string movieName)
        {
            var movies = from movie in _dbContext.Movies
                         where movie.Name.StartsWith(movieName)
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             ImageUrl = movie.ImageUrl
                         };
            return Ok(movies);
        }



        [Authorize(Roles="Admin")]
        [HttpPost]
        public IActionResult Post([FromForm] Movie movieObj)
        {
            var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot", guid + ".jpg");
            if (movieObj.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movieObj.Image.CopyTo(fileStream);
            }
            movieObj.ImageUrl = filePath.Remove(0, 7);
            _dbContext.Movies.Add(movieObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize(Roles ="Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Movie movieObj)
        {
            var obj = _dbContext.Movies.Find(id);
            if (obj == null)
            {
                return NotFound("No record found against this Id");
            }
            else
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", guid + ".jpg");
                if (movieObj.Image != null)
                {
                    var fileStream = new FileStream(filePath, FileMode.Create);
                    movieObj.Image.CopyTo(fileStream);
                    movieObj.ImageUrl = filePath.Remove(0, 7);
                }
                obj.Name = movieObj.Name;
                obj.Description = movieObj.Description;
                obj.Duration = movieObj.Duration;
                obj.PlayingDate = movieObj.PlayingDate;
                obj.PlayingTime = movieObj.PlayingTime;
                obj.Genre = movieObj.Genre;
                obj.TrailerUrl = movieObj.TrailerUrl;
                obj.TicketPrice = movieObj.TicketPrice;
                obj.Language = movieObj.Language;
                obj.Rating = movieObj.Rating;
                _dbContext.SaveChanges();
                return Ok("Record Updated Successfully");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
            {
                return NotFound("No record found against this Id");
            }
            else
            {
                _dbContext.Movies.Remove(movie);
                _dbContext.SaveChanges();
                return Ok("Record Deleted!");
            }
        }
    }
}
