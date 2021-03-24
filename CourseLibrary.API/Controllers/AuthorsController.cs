using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")] //or [Route("api/[controller]")]
    public class AuthorsController: ControllerBase
    {
        private ICourseLibraryRepository _courseLibraryRepository;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
        }

        [HttpGet()] //[HttpGet("api/authors")] no nee because we set [Route("api/authors")] up
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _courseLibraryRepository.GetAuthors();

            return Ok(authorsFromRepo);
        }

        [HttpGet("{authorId}")]
        public IActionResult GetAuthor(Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo==null)
                return NotFound();

            return Ok(authorFromRepo);
        }


    }
}
