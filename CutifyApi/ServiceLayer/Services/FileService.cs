using ServiceLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class FileService : IFileService
    {
        public async Task<string> ReadFileAsync(string path)
        {
            using StreamReader reader = new StreamReader(path);
            var body = await reader.ReadToEndAsync();
            return body;
        }
    }
}
