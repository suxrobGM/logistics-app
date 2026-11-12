using Logistics.Shared.Models;

namespace Logistics.API.Extensions;

public static class FormFileExtensions
{
    extension(IEnumerable<IFormFile>? files)
    {
        /// <summary>Wraps each non-empty file as a <see cref="FileUpload" />; empty files are dropped.</summary>
        public List<FileUpload> ToFileUploads()
        {
            if (files is null)
            {
                return [];
            }

            return files
                .Where(file => file.Length > 0)
                .Select(file => new FileUpload
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length
                })
                .ToList();
        }
    }
}
