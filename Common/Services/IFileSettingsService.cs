namespace Common.Services
{
    public interface IFileSettingsService
    {
        T GetSettings<T>(string path);
    }
}
