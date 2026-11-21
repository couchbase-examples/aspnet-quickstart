using System;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Org.Quickstart.API.Services
{
    public interface IInventoryScopeService
    {
        IScope GetInventoryScope();
    }

    /// <summary>
    /// Resolves and validates the configured inventory scope once, then hands the same
    /// <see cref="IScope"/> to every caller. Registered as a singleton; controllers are
    /// created per request, so without caching every HTTP request would repeat the
    /// bucket lookup and the scope-listing management call.
    /// A failed resolution is not cached, so the next request retries (e.g. after the
    /// travel-sample bucket finishes loading).
    /// </summary>
    public class InventoryScopeService : IInventoryScopeService
    {
        private readonly IBucketProvider _bucketProvider;
        private readonly IConfiguration _configuration;
        private readonly object _lock = new();
        private IScope _inventoryScope;

        public InventoryScopeService(IBucketProvider bucketProvider, IConfiguration configuration)
        {
            _bucketProvider = bucketProvider;
            _configuration = configuration;
        }

        public IScope GetInventoryScope()
        {
            var scope = _inventoryScope;
            if (scope != null)
            {
                return scope;
            }

            lock (_lock)
            {
                return _inventoryScope ??= ResolveInventoryScope();
            }
        }

        private IScope ResolveInventoryScope()
        {
            var bucketName = _configuration["Couchbase:BucketName"];
            var scopeName = _configuration["Couchbase:ScopeName"];

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new InvalidOperationException("Bucket name is not provided in the configuration.");
            }

            if (string.IsNullOrEmpty(scopeName))
            {
                throw new InvalidOperationException("Scope name is not provided in the configuration.");
            }

            IBucket bucket;
            try
            {
                bucket = _bucketProvider.GetBucketAsync(bucketName).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Ensure that you have the travel-sample bucket loaded in the cluster.");
            }

            if (!CheckScopeExists(bucket, scopeName))
            {
                throw new InvalidOperationException("Inventory scope does not exist in the bucket. Ensure that you have the inventory scope in your travel-sample bucket.");
            }

            return bucket.ScopeAsync(scopeName).GetAwaiter().GetResult();
        }

        private static bool CheckScopeExists(IBucket bucket, string scopeName)
        {
            var scopes = bucket.Collections.GetAllScopesAsync().GetAwaiter().GetResult();
            return scopes.Any(s => s.Name == scopeName);
        }
    }
}
