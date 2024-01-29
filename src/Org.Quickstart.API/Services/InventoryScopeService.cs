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

    public class InventoryScopeService : IInventoryScopeService
    {
        private readonly IBucketProvider _bucketProvider;
        private readonly IConfiguration _configuration;

        public InventoryScopeService(IBucketProvider bucketProvider, IConfiguration configuration)
        {
            _bucketProvider = bucketProvider;
            _configuration = configuration;
        }

        public IScope GetInventoryScope()
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
