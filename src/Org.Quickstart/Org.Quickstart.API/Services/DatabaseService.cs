using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Management.Buckets;
using Couchbase.Management.Collections;
using Couchbase.Query;
using Microsoft.Extensions.Logging;

namespace Org.Quickstart.API.Services
{
    public class DatabaseService
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
		private readonly ILogger<DatabaseService> _logger;        

	    public string BucketName { get; set; }
	    public string CollectionName { get; set; }
		public string ScopeName { get; set; }

        public DatabaseService(
	        IClusterProvider clusterProvider,
	        IBucketProvider bucketProvider,
			ILogger<DatabaseService> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
			BucketName = "user_profile";
			CollectionName = "profile";
			ScopeName = "_default";
			_logger = logger;
	    
        }

	    public async Task SetupDatabase()
	    {
			ICluster cluster = null;
			IBucket bucket = null;
			ICouchbaseCollection collection = null;

			//try to create bucket, if exists will just fail which is fine
			try 
			{
				cluster = await _clusterProvider.GetClusterAsync();	
				if (cluster != null)
				{
					var bucketSettings = new BucketSettings { 
						Name = BucketName, 
						BucketType = BucketType.Couchbase, 
						RamQuotaMB = 256 
					};

					await cluster.Buckets.CreateBucketAsync(bucketSettings);
					bucket = await _bucketProvider.GetBucketAsync(BucketName);
				}
				else 
					throw new System.Exception("Can't create bucket - cluster is null, please check database configuration.");
			}
			catch (BucketExistsException)
			{
				_logger.LogWarning($"Bucket {BucketName} already exists");
			}

			if (bucket != null) 
			{
				//try to create scope - if fails it's ok we are probably using default
				try
				{
					await bucket.Collections.CreateScopeAsync(new ScopeSpec(ScopeName));
				}
				catch(ScopeExistsException)
				{
					_logger.LogWarning($"Scope {ScopeName} already exists, probably default");
				}

				//try to create collection - if fails it's ok the collection probably exists
				try 
				{
					await bucket.Collections.CreateCollectionAsync(new CollectionSpec(ScopeName, CollectionName));
					collection = bucket.Collection(CollectionName);
				}
				catch (CollectionExistsException)
				{
					_logger.LogWarning($"Collection {CollectionName} already exists in {BucketName}.");
				}

				//try to create index - if fails it probably already exists
				try
				{
					var createIndexQuery = $"CREATE INDEX profile_lower_firstName ON default:{BucketName}._default.profile(lower(`firstName`));";
					var result = await cluster.QueryAsync<dynamic>(createIndexQuery);
					if (result.MetaData.Status != QueryStatus.Success || result.MetaData.Status != QueryStatus.Completed)
					{
						throw new System.Exception("Error create index didn't return proper results");
					}
				}
				catch (IndexExistsException)
				{
					_logger.LogWarning($"Collection {CollectionName} already exists in {BucketName}.");
				}
			}
			else 
				throw new System.Exception("Can't retreive bucket.");
	    }
    }
}
