using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abp.Authorization
{
    /// <summary>
    /// Null (and default) implementation of <see cref="IPermissionChecker"/>.
    /// </summary>
    public sealed class NullPermissionChecker : IPermissionChecker
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static NullPermissionChecker Instance { get; } = new NullPermissionChecker();

        public Task<bool> IsGrantedAsync(string permissionName)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsGrantedAsync(UserIdentifier user, string permissionName, long? branchId)
        {
            return Task.FromResult(true);
        }
        public bool IsGranted(string permissionName)
        {
            return true;
        }

        public bool IsGranted(UserIdentifier user, string permissionName, long? branchId)
        {
            return true;
        }

        public Task<List<string>> GetGrantedPermissionAsync(long userId)
        {
            return Task.FromResult(new List<string>());
        }
    }
}