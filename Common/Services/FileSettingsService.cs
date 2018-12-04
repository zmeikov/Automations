using System.IO;
using Newtonsoft.Json;

namespace Common.Services
{
    public class FileSettingsService : IFileSettingsService
    {

        public T GetSettings<T>(string path)
        {
            var settings = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

            return settings;
        }
    }
}