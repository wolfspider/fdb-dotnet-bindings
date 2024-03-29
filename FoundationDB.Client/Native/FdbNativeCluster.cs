﻿#region BSD License
/* Copyright (c) 2013-2018, Doxense SAS
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
	* Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.
	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.
	* Neither the name of Doxense nor the
	  names of its contributors may be used to endorse or promote products
	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

namespace FoundationDB.Client.Native
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Doxense.Diagnostics.Contracts;
	using FoundationDB.Client.Core;

	/// <summary>Wraps a native FDBCluster* handle</summary>
	internal sealed class FdbNativeCluster : IFdbClusterHandler
	{
		private readonly ClusterHandle m_handle;
		private DatabaseHandle d_handle;

		public FdbNativeCluster(ClusterHandle handle)
		{
			Contract.Requires(handle != null);
			m_handle = handle;
		}

		public static Task<IFdbClusterHandler> CreateClusterAsync(string clusterFile, CancellationToken ct)
		{
			var future = FdbNative.CreateCluster(clusterFile);

			return FdbFuture.CreateTaskFromHandle(future,
				(h) =>
				{
					var err = FdbNative.FutureGetCluster(h, out ClusterHandle cluster);
					if (err != FdbError.Success)
					{
						cluster.Dispose();
						throw Fdb.MapToException(err);
					}
					var handler = new FdbNativeCluster(cluster);
					return (IFdbClusterHandler) handler;
				},
				ct
			);
		}

		internal DatabaseHandle DHandle => d_handle;

		internal ClusterHandle Handle => m_handle;

		public bool IsInvalid => m_handle.IsInvalid;

		public bool IsClosed => m_handle.IsClosed;

		public void SetOption(FdbClusterOption option, Slice data)
		{
			Fdb.EnsureNotOnNetworkThread();

			unsafe
			{
				fixed (byte* ptr = data)
				{
					Fdb.DieOnError(FdbNative.ClusterSetOption(m_handle, option, ptr, data.Count));
				}
			}
		}

		public DatabaseHandle CreateDatabase(string databaseName)
		{

			FdbNative.CreateDatabase(databaseName, out d_handle);
			return d_handle;
		
		}


		public Task<IFdbDatabaseHandler> OpenDatabaseAsync(string databaseName, CancellationToken ct)
		{
			if (ct.IsCancellationRequested) return Task.FromCanceled<IFdbDatabaseHandler>(ct);

			return Task.Run(() => {
				var handler = new FdbNativeDatabase(CreateDatabase("/usr/local/etc/foundationdb/fdb.cluster"));
				return (IFdbDatabaseHandler) handler;
			});

		}

		public void Dispose()
		{
			m_handle?.Dispose();
		}

	}


}
