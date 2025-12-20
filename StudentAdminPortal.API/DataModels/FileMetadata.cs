using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentAdminPortal.API.DataModels
{
    public class FileMetadata
    {
        
        public Guid Id { get; set; } = Guid.NewGuid();

      
        public string FileName { get; set; }

        public string ContentType { get; set; }
    
        public long FileSize { get; set; }

        public byte[] FileContent { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

    }
}
