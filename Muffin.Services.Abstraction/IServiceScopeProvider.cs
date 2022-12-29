﻿using Microsoft.Extensions.DependencyInjection;

namespace Muffin.Services.Abstraction
{
    public interface IServiceScopeProvider
    {
        void PrepareServiceScope(string scopeIdentifier, IServiceScope serviceScope);
        void RegisterScope<T>(Action<IServiceProvider> scopeBuilder);
        void RegisterScope(string scopeIdentifier, Action<IServiceProvider> scopeBuilder);
    }
}