using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace StudentAdminPortal.API.DataModels
{
    public class UploadedFile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        [Column(TypeName = "varbinary(max)")] // Explicitly map to VARBINARY(MAX)
        public byte[] FileContent { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
