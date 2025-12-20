using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAdminPortal.API.DataModels;
using StudentAdminPortal.API.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudentAdminPortal.API.Controllers
{
    public class FilesController : Controller
    {

        private readonly StudentAdminContext _context;
       // private readonly IFileRepository _fileRepository;

        public FilesController(StudentAdminContext context, IMapper mapper)
        {
            _context = context;
               
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Basic file validation (you'll want more robust validation)
            if (file.Length > 20 * 1024 * 1024) // Example: Max 20 MB for database storage
            {
                return BadRequest("File size exceeds 20MB limit for direct database storage.");
            }

            // Read file content into a byte array
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var uploadedFile = new UploadedFile
            {
                FileName = Path.GetFileName(file.FileName), // Use original file name
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileContent = fileBytes,
                UploadDate = DateTime.UtcNow // Use UtcNow for consistency
            };

            _context.UploadedFiles.Add(uploadedFile);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "File uploaded successfully!", FileId = uploadedFile.Id });
        }



        [HttpGet("download")]
        public async Task<ActionResult<IEnumerable<FileMetadata>>> GetFiles()
        {
            var fileDtos = await _context.UploadedFiles.Select(f => new FileMetadata
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                FileSize = f.FileSize,
                FileContent = f.FileContent,
                UploadDate = f.UploadDate
            }).ToListAsync();

            return Ok(fileDtos);
        }



        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> Download(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest("File ID not provided.");
            }

            var uploadedFile = await _context.UploadedFiles
                                             .FirstOrDefaultAsync(f => f.Id == fileId);

            if (uploadedFile == null)
            {
                return NotFound("File not found in database.");
            }

            // Corrected the File method call to use the file content as the first argument
            return File(uploadedFile.FileContent, uploadedFile.ContentType, uploadedFile.FileName);
        }
    }
}