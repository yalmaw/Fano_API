//using Microsoft.EntityFrameworkCore;
//using StudentAdminPortal.API.DataModels;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace StudentAdminPortal.API.Repositories
//{
//    public class FileRepository : IFileRepository
//    {
//        private readonly StudentAdminContext context;

//        public FileRepository(StudentAdminContext context)
//        {
//            this.context = context;
//        }

//        public async Task<IEnumerable<FileMetadata>> GetAllFilesAsync()
//        {
//            // Adjusted return type to match the interface definition
//            return await context.FileMetadata.ToListAsync();
//        }

//        public async Task<FileMetadata?> GetFileByIdAsync(Guid id)
//        {
//            // Adjusted return type to match the interface definition
//            return await context.Set<FileMetadata>().FindAsync(id);
//        }
//    }
//}
