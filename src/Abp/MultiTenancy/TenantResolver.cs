using System;
using System.Linq;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Runtime;
using Castle.Core.Logging;

namespace Abp.MultiTenancy
{
    public class TenantResolver : ITenantResolver, ITransientDependency
    {
        private const string AmbientScopeContextKey = "Abp.MultiTenancy.TenantResolver.Resolving";

        public ILogger Logger { get; set; }

        private readonly IMultiTenancyConfig _multiTenancy;
        private readonly IIocResolver _iocResolver;
        private readonly ITenantStore _tenantStore;
        private readonly ITenantResolverCache _cache;
        private readonly IAmbientScopeProvider<bool> _ambientScopeProvider;

        public TenantResolver(
            IMultiTenancyConfig multiTenancy,
            IIocResolver iocResolver,
            ITenantStore tenantStore,
            ITenantResolverCache cache,
            IAmbientScopeProvider<bool> ambientScopeProvider)
        {
            _multiTenancy = multiTenancy;
            _iocResolver = iocResolver;
            _tenantStore = tenantStore;
            _cache = cache;
            _ambientScopeProvider = ambientScopeProvider;

            Logger = NullLogger.Instance;
        }

        public long? ResolveTenantId()
        {
            if (!_multiTenancy.Resolvers.Any())
            {
                return null;
            }

            if (_ambientScopeProvider.GetValue(AmbientScopeContextKey))
            {
                //Preventing recursive call of ResolveTenantId
                return null;
            }

            using (_ambientScopeProvider.BeginScope(AmbientScopeContextKey, true))
            {
                var cacheItem = _cache.Value;
                if (cacheItem != null)
                {
                    return cacheItem.TenantId;
                }

                var tenantId = GetTenantIdFromContributors();
                _cache.Value = new TenantResolverCacheItem(tenantId);
                return tenantId;
            }
        }

        private long? GetTenantIdFromContributors()
        {
            foreach (var resolverType in _multiTenancy.Resolvers)
            {
                using (var resolver = _iocResolver.ResolveAsDisposable<ITenantResolveContributor>(resolverType))
                {
                    long? tenantId;

                    try
                    {
                        tenantId = resolver.Object.ResolveTenantId();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString(), ex);
                        continue;
                    }

                    if (tenantId == null)
                    {
                        continue;
                    }

                    if (_tenantStore.Find(tenantId.Value) == null)
                    {
                        continue;
                    }

                    return tenantId;
                }
            }

            return null;
        }
    }

    public class BranchResolver : IBranchResolver, ITransientDependency
    {
        private const string AmbientScopeContextKey = "Abp.MultiTenancy.BranchResolver.Resolving";

        public ILogger Logger { get; set; }

        private readonly IMultiTenancyConfig _multiTenancy;
        private readonly IIocResolver _iocResolver;
        private readonly ITenantStore _tenantStore;
        private readonly IBranchResolverCache _cache;
        private readonly IAmbientScopeProvider<bool> _ambientScopeProvider;

        public BranchResolver(
            IMultiTenancyConfig multiTenancy,
            IIocResolver iocResolver,
            ITenantStore tenantStore,
            IBranchResolverCache cache,
            IAmbientScopeProvider<bool> ambientScopeProvider)
        {
            _multiTenancy = multiTenancy;
            _iocResolver = iocResolver;
            _tenantStore = tenantStore;
            _cache = cache;
            _ambientScopeProvider = ambientScopeProvider;

            Logger = NullLogger.Instance;
        }

        public long? ResolveBranchId()
        {
            if (!_multiTenancy.Resolvers.Any())
            {
                return null;
            }

            if (_ambientScopeProvider.GetValue(AmbientScopeContextKey))
            {
                //Preventing recursive call of ResolveTenantId
                return null;
            }

            using (_ambientScopeProvider.BeginScope(AmbientScopeContextKey, true))
            {
                var cacheItem = _cache.Value;
                if (cacheItem != null)
                {
                    return cacheItem.BranchId;
                }

                var tenantId = GetBranchIdFromContributors();
                _cache.Value = new BranchResolverCacheItem(tenantId);
                return tenantId;
            }
        }

        private long? GetBranchIdFromContributors()
        {
            foreach (var resolverType in _multiTenancy.BranchResolvers)
            {
                using (var resolver = _iocResolver.ResolveAsDisposable<IBranchResolveContributor>(resolverType))
                {
                    long? branchId;

                    try
                    {
                        branchId = resolver.Object.ResolveBranchId();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString(), ex);
                        continue;
                    }

                    if (branchId == null)
                    {
                        continue;
                    }

                    //if (_tenantStore.Find(tenantId.Value) == null)
                    //{
                    //    continue;
                    //}

                    return branchId;
                }
            }

            return null;
        }
    }
}