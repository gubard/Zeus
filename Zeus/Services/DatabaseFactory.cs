using Gaia.Services;
using Nestor.Db.LiteDb.Services;
using UltraLiteDB;

namespace Zeus.Services;

public sealed class GuidDatabaseFactory
{
    public GuidDatabaseFactory(IStorageService storageService, string appName)
    {
        _storageService = storageService;
        _appName = appName;
    }

    public IDatabase Create(Guid id)
    {
        InitDbContext(id);

        return _cache[id];
    }

    private readonly Dictionary<Guid, IDatabase> _cache = new();
    private readonly IStorageService _storageService;
    private readonly string _appName;

    private FileInfo CreateDbFile(Guid id)
    {
        return new($"{_storageService.GetDbDirectory()}/{_appName}/{id}.litedb");
    }

    private void InitDbContext(Guid id)
    {
        if (_cache.ContainsKey(id))
        {
            return;
        }

        var file = CreateDbFile(id);
        var ultra = new UltraLiteDatabase(file.FullName);
        var database = new Database(ultra);
        _cache.Add(id, database);
    }
}
