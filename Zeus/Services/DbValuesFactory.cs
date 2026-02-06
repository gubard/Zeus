using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Microsoft.AspNetCore.Http;
using Zeus.Helpers;

namespace Zeus.Services;

public sealed class DbValuesFactory : IFactory<DbValues>
{
    public DbValuesFactory(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public DbValues Create()
    {
        return _accessor.HttpContext.ThrowIfNull().GetRequestValues();
    }

    private readonly IHttpContextAccessor _accessor;
}
