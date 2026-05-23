namespace Client.Interfaces.Authorisation
{
    public interface ISecureStorageService
    {
        Task SetAsync<T>(string key, T value);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task SetLocalAsync<T>(string key, T value);
        Task<T?> GetLocalAsync<T>(string key);
        Task RemoveLocalAsync(string key);
    }
}