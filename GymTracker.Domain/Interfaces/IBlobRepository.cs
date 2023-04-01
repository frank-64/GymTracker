using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain
{
    public interface IBlobRepository
    {
        public Task UploadBlobAsync<T>(T objectToUpload, string blobName);
        public Task<bool> CheckIfBlobExists(string blobPath);
        public Task<T> GetBlob<T>(string blobName);
    }
}
