using Microsoft.AspNetCore.Authorization;
using System;

namespace Muffin.AspNetCore.Authorization.Permissions
{
    public class RequiredPermissionsAttribute : AuthorizeAttribute
    {
        internal const string PolicyPrefix = "PERMISSION_";
        private const string Separator = ",";

        public RequiredPermissionsAttribute(
            PermissionOperator permissionOperator, params string[] permissions)
        {
            Policy = $"{PolicyPrefix}{(int)permissionOperator}{Separator}{string.Join(Separator, permissions)}";
        }

        public RequiredPermissionsAttribute(string permission)
        {
            Policy = $"{PolicyPrefix}{(int)PermissionOperator.And}{Separator}{permission}";
        }

        public static PermissionOperator GetOperatorFromPolicy(string policyName)
        {
            var @operator = int.Parse(policyName.AsSpan(PolicyPrefix.Length, 1));
            return (PermissionOperator)@operator;
        }

        public static string[] GetPermissionsFromPolicy(string policyName)
        {
            return policyName.Substring(PolicyPrefix.Length + 2)
                .Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public enum PermissionOperator
    {
        And = 1, Or = 2
    }
}
