using System.Linq;
using Abp.Collections.Extensions;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MultiTenancy;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;

namespace Abp.AspNetCore.MultiTenancy
{
    public class HttpHeaderTenantResolveContributor : ITenantResolveContributor, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        public HttpHeaderTenantResolveContributor(
            IHttpContextAccessor httpContextAccessor,
            IMultiTenancyConfig multiTenancyConfig)
        {
            _httpContextAccessor = httpContextAccessor;
            _multiTenancyConfig = multiTenancyConfig;

            Logger = NullLogger.Instance;
        }

        public long? ResolveTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var tenantIdHeader = httpContext.Request.Headers[_multiTenancyConfig.TenantIdResolveKey];
            if (tenantIdHeader == string.Empty || tenantIdHeader.Count < 1)
            {
                return null;
            }

            if (tenantIdHeader.Count > 1)
            {
                Logger.Warn(
                    $"HTTP request includes more than one {_multiTenancyConfig.TenantIdResolveKey} header value. First one will be used. All of them: {tenantIdHeader.JoinAsString(", ")}"
                    );
            }

            return int.TryParse(tenantIdHeader.First(), out var tenantId) ? tenantId : (int?)null;
        }
    }

    public class HttpHeaderTenantCodeResolveContributor : ITenantResolveContributor, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly ITenantStore tenantStore;

        public HttpHeaderTenantCodeResolveContributor(
            IHttpContextAccessor httpContextAccessor,
            IMultiTenancyConfig multiTenancyConfig,
            ITenantStore tenantStore)
        {
            _httpContextAccessor = httpContextAccessor;
            _multiTenancyConfig = multiTenancyConfig;
            this.tenantStore = tenantStore;
            Logger = NullLogger.Instance;
        }

        public long? ResolveTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var tenantCodeHeader = httpContext.Request.Headers[_multiTenancyConfig.TenantCodeResolveKey];
            if (tenantCodeHeader == string.Empty || tenantCodeHeader.Count < 1)
            {
                return null;
            }

            if (tenantCodeHeader.Count > 1)
            {
                Logger.Warn(
                    $"HTTP request includes more than one {_multiTenancyConfig.TenantIdResolveKey} header value. First one will be used. All of them: {tenantCodeHeader.JoinAsString(", ")}"
                    );
            }

            var code = tenantCodeHeader.First();
            var tenantInfo = tenantStore.Find(code);
            if (tenantInfo == null)
            {
                return null;
            }

            return tenantInfo.Id;
        }
    }


    public class HttpHeaderBranchResolveContributor : IBranchResolveContributor, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        public HttpHeaderBranchResolveContributor(
            IHttpContextAccessor httpContextAccessor,
            IMultiTenancyConfig multiTenancyConfig)
        {
            _httpContextAccessor = httpContextAccessor;
            _multiTenancyConfig = multiTenancyConfig;

            Logger = NullLogger.Instance;
        }

        public long? ResolveBranchId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var branchIdHeader = httpContext.Request.Headers[_multiTenancyConfig.BranchIdResolveKey];
            if (branchIdHeader == string.Empty || branchIdHeader.Count < 1)
            {
                return null;
            }

            if (branchIdHeader.Count > 1)
            {
                Logger.Warn(
                    $"HTTP request includes more than one {_multiTenancyConfig.BranchIdResolveKey} header value. First one will be used. All of them: {branchIdHeader.JoinAsString(", ")}"
                    );
            }

            return long.TryParse(branchIdHeader.First(), out var branchId) ? branchId : (long?)null;
        }
    }
}
