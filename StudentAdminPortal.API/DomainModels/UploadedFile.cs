using System;

namespace StudentAdminPortal.API.DomainModels
{
    public class UploadedFile
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileContent { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
