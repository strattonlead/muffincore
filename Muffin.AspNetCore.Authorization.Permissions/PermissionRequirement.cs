using Microsoft.AspNetCore.Authorization;
using System;

namespace Muffin.AspNetCore.Authorization.Permissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public static string ClaimType => "Permissions";

        // 1 - The operator
        public PermissionOperator PermissionOperator { get; }

        // 2 - The list of permissions passed
        public string[] Permissions { get; }

        public PermissionRequirement(
            PermissionOperator permissionOperator, string[] permissions)
        {
            if (permissions.Length == 0)
                throw new ArgumentException("At least one permission is required.", nameof(permissions));

            PermissionOperator = permissionOperator;
            Permissions = permissions;
        }
    }
}


//public static class ClaimsHelper
//{
//    public static void GetPermissions(this List<RoleClaimsViewModel> allPermissions, Type policy, string roleId)
//    {
//        FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
//        foreach (FieldInfo fi in fields)
//        {
//            allPermissions.Add(new RoleClaimsViewModel { Value = fi.GetValue(null).ToString(), Type = "Permissions" });
//        }
//    }
//    public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string permission)
//    {
//        var allClaims = await roleManager.GetClaimsAsync(role);
//        if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
//        {
//            await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
//        }
//    }
//}