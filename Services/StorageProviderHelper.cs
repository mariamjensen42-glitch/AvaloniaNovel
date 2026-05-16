using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace AgentNovel.Services;

public static class StorageProviderHelper
{
    public static async Task<IStorageFolder?> TryGetFolderFromPathAsync(
        IStorageProvider storageProvider, string path)
    {
        try
        {
            return await storageProvider.TryGetFolderFromPathAsync(path);
        }
        catch
        {
            return null;
        }
    }
}
