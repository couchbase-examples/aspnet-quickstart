using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Management.Collections;
using Microsoft.Extensions.Configuration;
using Org.Quickstart.API.Services;
using Xunit;

namespace Org.Quickstart.IntegrationTests.ServiceTests;

// Runs without a Couchbase cluster: the SDK interfaces are faked with DispatchProxy.
public class InventoryScopeServiceTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Couchbase:BucketName"] = "travel-sample",
            ["Couchbase:ScopeName"] = "inventory"
        })
        .Build();

    [Fact]
    public void ResolvesScopeOnceAndReusesIt()
    {
        var cluster = new FakeCluster();
        var service = new InventoryScopeService(cluster.BucketProvider, Configuration);

        var first = service.GetInventoryScope();
        var second = service.GetInventoryScope();
        var third = service.GetInventoryScope();

        Assert.Same(cluster.Scope, first);
        Assert.Same(first, second);
        Assert.Same(first, third);
        Assert.Equal(1, cluster.GetBucketCalls);
        Assert.Equal(1, cluster.GetAllScopesCalls);
    }

    [Fact]
    public void FailedResolutionIsNotCached()
    {
        var cluster = new FakeCluster { BucketAvailable = false };
        var service = new InventoryScopeService(cluster.BucketProvider, Configuration);

        var ex = Assert.Throws<InvalidOperationException>(() => service.GetInventoryScope());
        Assert.Contains("travel-sample bucket", ex.Message);

        cluster.BucketAvailable = true;

        Assert.Same(cluster.Scope, service.GetInventoryScope());
        Assert.Equal(2, cluster.GetBucketCalls);
    }

    [Fact]
    public void MissingScopeIsReported()
    {
        var cluster = new FakeCluster { ScopeNames = new[] { "_default" } };
        var service = new InventoryScopeService(cluster.BucketProvider, Configuration);

        var ex = Assert.Throws<InvalidOperationException>(() => service.GetInventoryScope());
        Assert.Contains("Inventory scope does not exist", ex.Message);
    }

    private sealed class FakeCluster
    {
        public bool BucketAvailable { get; set; } = true;
        public string[] ScopeNames { get; set; } = { "_default", "inventory" };
        public int GetBucketCalls { get; private set; }
        public int GetAllScopesCalls { get; private set; }

        public IScope Scope { get; }
        public IBucketProvider BucketProvider { get; }

        public FakeCluster()
        {
            Scope = Fake<IScope>.Create(_ => throw new NotSupportedException());

            var collections = Fake<ICouchbaseCollectionManager>.Create(method =>
            {
                if (method.Name != nameof(ICouchbaseCollectionManager.GetAllScopesAsync))
                    throw new NotSupportedException(method.Name);
                GetAllScopesCalls++;
                var specs = new List<ScopeSpec>();
                foreach (var name in ScopeNames) specs.Add(new ScopeSpec(name));
                return Task.FromResult<IEnumerable<ScopeSpec>>(specs);
            });

            var bucket = Fake<IBucket>.Create(method => method.Name switch
            {
                "get_Collections" => collections,
                nameof(IBucket.ScopeAsync) => new ValueTask<IScope>(Scope),
                _ => throw new NotSupportedException(method.Name)
            });

            BucketProvider = Fake<IBucketProvider>.Create(method =>
            {
                if (method.Name != nameof(IBucketProvider.GetBucketAsync))
                    throw new NotSupportedException(method.Name);
                GetBucketCalls++;
                if (!BucketAvailable)
                    return ValueTask.FromException<IBucket>(new Exception("bucket not found"));
                return new ValueTask<IBucket>(bucket);
            });
        }
    }

    public class Fake<T> : DispatchProxy where T : class
    {
        private Func<MethodInfo, object> _handler;

        public static T Create(Func<MethodInfo, object> handler)
        {
            var proxy = Create<T, Fake<T>>();
            ((Fake<T>)(object)proxy)._handler = handler;
            return proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args) => _handler(targetMethod);
    }
}
