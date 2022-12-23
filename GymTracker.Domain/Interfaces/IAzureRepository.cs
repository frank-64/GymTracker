using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain
{
    public interface IAzureRepository
    {
        public Task UploadBlobAsync(Stream stream, string blobName);
        public Task<bool> CheckIfBlobExists(string blobPath);
        public Task<Stream> GetBlob(string blobName);
    }
}
