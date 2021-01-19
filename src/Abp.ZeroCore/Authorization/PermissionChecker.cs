using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Castle.Core.Logging;

namespace Abp.Authorization
{
    /// <summary>
    /// Application should inherit this class to implement <see cref="IPermissionChecker"/>.
    /// </summary>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    public class PermissionChecker<TRole, TUser> : IPermissionChecker, ITransientDependency, IIocManagerAccessor
        where TRole : AbpRole<TUser>, new()
        where TUser : AbpUser<TUser>
    {
        private readonly AbpUserManager<TRole, TUser> _userManager;
        private readonly AbpRoleManager<TRole, TUser> roleManager;

        public IIocManager IocManager { get; set; }

        public ILogger Logger { get; set; }

        public IAbpSession AbpSession { get; set; }

        public ICurrentUnitOfWorkProvider CurrentUnitOfWorkProvider { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PermissionChecker(AbpUserManager<TRole, TUser> userManager, AbpRoleManager<TRole, TUser> roleManager)
        {
            _userManager = userManager;
            this.roleManager = roleManager;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        public virtual async Task<bool> IsGrantedAsync(string permissionName)
        {
            return AbpSession.UserId.HasValue && await IsGrantedAsync(AbpSession.UserId.Value, permissionName, AbpSession.BranchId);
        }

        public virtual bool IsGranted(string permissionName)
        {
            return AbpSession.UserId.HasValue && IsGranted(AbpSession.UserId.Value, permissionName, AbpSession.BranchId);
        }

        public virtual async Task<bool> IsGrantedAsync(long userId, string permissionName, long? branchId)
        {
            return await _userManager.IsGrantedAsync(userId, permissionName, branchId);
        }

        public virtual bool IsGranted(long userId, string permissionName, long? branchId)
        {
            return _userManager.IsGranted(userId, permissionName, branchId);
        }

        [UnitOfWork]
        public virtual async Task<bool> IsGrantedAsync(UserIdentifier user, string permissionName, long? branchId)
        {
            if (CurrentUnitOfWorkProvider?.Current == null)
            {
                return await IsGrantedAsync(user.UserId, permissionName, branchId);
            }

            using (CurrentUnitOfWorkProvider.Current.SetTenantId(user.TenantId))
            {
                return await IsGrantedAsync(user.UserId, permissionName, branchId);
            }
        }

        [UnitOfWork]
        public virtual bool IsGranted(UserIdentifier user, string permissionName, long? branchId)
        {
            if (CurrentUnitOfWorkProvider?.Current == null)
            {
                return IsGranted(user.UserId, permissionName, branchId);
            }

            using (CurrentUnitOfWorkProvider.Current.SetTenantId(user.TenantId))
            {
                return IsGranted(user.UserId, permissionName, branchId);
            }
        }

        public Task<List<string>> GetGrantedPermissionAsync(long userId)
        {
            return GetGrantedPermissionAsync(userId, AbpSession.BranchId);
        }

        private async Task<List<string>> GetGrantedPermissionAsync(long userId, long? branchId)
        {
            var cacheItem = await _userManager.GetUserPermissionCacheItemAsync(userId, branchId);
            if (cacheItem == null)
            {
                return new List<string>();
            }
            var permissions = new List<string>();
            foreach (var item in cacheItem.RoleIds)
            {
                var g = await roleManager.GetGrantedPermissionsAsync(item);
                permissions.AddRange(g.Select(e => e.Name));
            }
            return permissions.Union(cacheItem.GrantedPermissions).ToList();
        }
    }
}
