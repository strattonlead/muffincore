using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Services.V8
{
    public interface IJavaScriptExecutor
    {
        Task<T> Execute<T>(string script);
        Task<T> Execute<T>(string script, Dictionary<string, object> properties);
        Task<T> Execute<T>(string script, CancellationToken cancellationToken);
        Task<T> Execute<T>(string script, Dictionary<string, object> properties, CancellationToken cancellationToken);
    }
}
