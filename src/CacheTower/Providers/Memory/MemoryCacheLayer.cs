using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheTower.Internal;

namespace CacheTower.Providers.Memory
{
	/// <remarks>
	/// The <see cref="MemoryCacheLayer"/> allows fast, local memory caching.
	/// Cached data is stored as a reference internally, rather than serialized like other caching systems do.
	/// This provides far greater performance than other in-memory solutions.
	/// <para>
	/// Because of the stored reference, it is strongly recommend to not modify instances of a cached value.
	/// Instead, if there is an updated state to store, use the appropriate setter methods on the cache stack or layer.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="ICacheLayer"/>
	public class MemoryCacheLayer : ILocalCacheLayer
	{
		private readonly ConcurrentDictionary<string, ICacheEntry> Cache = new(StringComparer.Ordinal);

		/// <inheritdoc/>
		public ValueTask CleanupAsync()
		{
			var currentTime = DateTimeProvider.Now;

			foreach (var cachePair in Cache)
			{
				var cacheEntry = cachePair.Value;
				if (cacheEntry.Expiry < currentTime)
				{
					Cache.TryRemove(cachePair.Key, out _);
				}
			}

			return new ValueTask();
		}

		/// <inheritdoc/>
		public ValueTask EvictAsync(string cacheKey)
		{
			Cache.TryRemove(cacheKey, out _);
			return new ValueTask();
		}

		/// <inheritdoc/>
		public ValueTask FlushAsync()
		{
			Cache.Clear();
			return new ValueTask();
		}

		/// <inheritdoc/>
		public ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
		{
			if (Cache.TryGetValue(cacheKey, out var cacheEntry))
			{
				if(typeof(T) is object)
				{
					var value = cacheEntry.GetType().GetProperty("Value").GetValue(cacheEntry);

					var x = new CacheEntry<T>((T)value, cacheEntry.Expiry);

					return new ValueTask<CacheEntry<T>?>(x);
				}

				return new ValueTask<CacheEntry<T>?>(cacheEntry as CacheEntry<T>);
			}

			return new ValueTask<CacheEntry<T>?>(default(CacheEntry<T>));
		}

		/// <inheritdoc/>
		public ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			return new ValueTask<bool>(true);
		}

		/// <inheritdoc/>
		public ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			Cache[cacheKey] = cacheEntry;
			return new ValueTask();
		}

		/// <inheritdoc/>
		public ValueTask<IEnumerable<string>> GetKeys()
		{
			return new ValueTask<IEnumerable<string>>(Cache.Keys);
		}
	}
}
