using System;

namespace Muffin.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ProtectedAttribute : Attribute
    { }
}
