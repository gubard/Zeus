using System.Runtime.CompilerServices;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Nestor.Db.LiteDb.Services;
using UltraLiteDB;

namespace Zeus.Services;

public sealed class GuidDatabaseFactory : IDatabaseFactory
{
    public GuidDatabaseFactory(
        IStorageService storageService,
        IFactory<DbValues> dbValuesFactory,
        string appName
    )
    {
        _storageService = storageService;
        _dbValuesFactory = dbValuesFactory;
        _appName = appName;
    }

    public ConfiguredValueTaskAwaitable<IDatabase> CreateAsync(CancellationToken ct)
    {
        var dbValues = _dbValuesFactory.Create();
        InitDbContext(dbValues.UserId);

        return TaskHelper.FromResult(Cache[dbValues.UserId]);
    }

    private static readonly Dictionary<Guid, IDatabase> Cache = new();
    private readonly IFactory<DbValues> _dbValuesFactory;
    private readonly IStorageService _storageService;
    private readonly string _appName;

    private FileInfo CreateDbFile(Guid userId)
    {
        return new($"{_storageService.GetDbDirectory()}/{_appName}/{userId}.litedb");
    }

    private void InitDbContext(Guid userId)
    {
        if (Cache.ContainsKey(userId))
        {
            return;
        }

        var file = CreateDbFile(userId);
        var ultra = new UltraLiteDatabase(file.FullName);
        var database = new Database(ultra);
        Cache.Add(userId, database);
    }
}
