﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Abp.Authorization
{
    /// <summary>
    /// Used to grant/deny a permission for a role or user.
    /// </summary>
    [Table("AbpPermissions")]
    public abstract class PermissionSetting : CreationAuditedEntity<long>, IMayHaveTenant, IMayHaveBranch
    {
        /// <summary>
        /// Maximum length of the <see cref="Name"/> field.
        /// </summary>
        public const int MaxNameLength = 128;

        [Column("tenant_id")]
        public virtual long? TenantId { get; set; }

        /// <summary>
        /// Unique name of the permission.
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public virtual string Name { get; set; }

        /// <summary>
        /// Is this role granted for this permission.
        /// Default value: true.
        /// </summary>
        public virtual bool IsGranted { get; set; }
        public long? BranchId { get; set; }

        /// <summary>
        /// Creates a new <see cref="PermissionSetting"/> entity.
        /// </summary>
        protected PermissionSetting()
        {
            IsGranted = true;
        }
    }
}
