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

namespace FoundationDB.DependencyInjection
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using FoundationDB.Client;
	using JetBrains.Annotations;

	[PublicAPI]
	public static class FdbDatabaseProviderExtensions
	{

		#region IFdbDatabaseProvider extensions...

		/// <summary>Runs a transactional lambda function inside a read-only transaction, which can be executed more than once if any retryable error occurs.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Asynchronous handler that will be retried until it succeeds, or a non-recoverable error occurs.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <remarks>
		/// Since the handler can run more than once, and that there is no guarantee that the transaction commits once it returns, you MAY NOT mutate any global state (counters, cache, global dictionary) inside this lambda!
		/// You must wait for the Task to complete successfully before updating the global state of the application.
		/// </remarks>
		public static async Task<TResult> ReadAsync<TResult>(
			[NotNull] this IFdbDatabaseProvider provider,
			[NotNull, InstantHandle] Func<IFdbReadOnlyTransaction, Task<TResult>> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			return await db.ReadAsync(handler, ct).ConfigureAwait(false);
		}

		/// <summary>Run an idempotent transactional block that returns a value, inside a read-write transaction, which can be executed more than once if any retry-able error occurs.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Idempotent asynchronous lambda function that will be retried until the transaction commits, or a non-recoverable error occurs. The returned value of the last call will be the result of the operation.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <returns>Result of the lambda function if the transaction committed successfully.</returns>
		/// <remarks>
		/// You do not need to commit the transaction inside the handler, it will be done automatically.
		/// Since the <paramref name="handler"/> can run more than once, and that there is no guarantee that the transaction commits once it returns, you MAY NOT mutate any global state (counters, cache, global dictionary) inside this lambda!
		/// You must wait for the Task to complete successfully before updating the global state of the application.
		/// </remarks>
		public static async Task<TResult> ReadWriteAsync<TResult>(
			[NotNull] this IFdbDatabaseProvider provider,
			[NotNull, InstantHandle] Func<IFdbTransaction, Task<TResult>> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			return await db.ReadWriteAsync(handler, ct).ConfigureAwait(false);
		}

		/// <summary>Runs a query inside a read-only transaction context, with retry-logic.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Lambda function that returns an async enumerable. The function may be called multiple times if the transaction conflicts.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <returns>Task returning the list of all the elements of the async enumerable returned by the last successful call to <paramref name="handler"/>.</returns>
		public static async Task<List<TResult>> QueryAsync<TResult>(
			[NotNull] this IFdbDatabaseProvider provider,
			[NotNull, InstantHandle] Func<IFdbReadOnlyTransaction, Doxense.Linq.IAsyncEnumerable<TResult>> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			return await db.QueryAsync(handler, ct).ConfigureAwait(false);
		}

		/// <summary>Run an idempotent transaction block inside a write-only transaction, which can be executed more than once if any retry-able error occurs.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Idempotent handler that should only call write methods on the transaction, and may be retried until the transaction commits, or a non-recoverable error occurs.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <remarks>
		/// You do not need to commit the transaction inside the handler, it will be done automatically.
		/// Since the <paramref name="handler"/> can run more than once, and that there is no guarantee that the transaction commits once it returns, you MAY NOT mutate any global state (counters, cache, global dictionary) inside this lambda!
		/// You must wait for the Task to complete successfully before updating the global state of the application.
		/// </remarks>
		public static async Task WriteAsync(
			[NotNull] this IFdbDatabaseProvider provider,
			[NotNull, InstantHandle] Action<IFdbTransaction> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			await db.WriteAsync(handler, ct).ConfigureAwait(false);
		}

		#endregion

		#region IFdbDatabaseScopeProvider extensions...

		/// <summary>Runs a transactional lambda function inside a read-only transaction, which can be executed more than once if any retryable error occurs.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Asynchronous handler that will be retried until it succeeds, or a non-recoverable error occurs.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <remarks>
		/// Since the handler can run more than once, and that there is no guarantee that the transaction commits once it returns, you MAY NOT mutate any global state (counters, cache, global dictionary) inside this lambda!
		/// You must wait for the Task to complete successfully before updating the global state of the application.
		/// </remarks>
		public static async Task<TResult> ReadAsync<TResult>(
			[NotNull] this IFdbDatabaseScopeProvider provider,
			[NotNull, InstantHandle] Func<IFdbReadOnlyTransaction, Task<TResult>> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			return await db.ReadAsync(handler, ct).ConfigureAwait(false);
		}

		/// <summary>Run an idempotent transactional block that returns a value, inside a read-write transaction, which can be executed more than once if any retryable error occurs.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Idempotent asynchronous lambda function that will be retried until the transaction commits, or a non-recoverable error occurs. The returned value of the last call will be the result of the operation.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <returns>Result of the lambda function if the transaction committed successfully.</returns>
		/// <remarks>
		/// You do not need to commit the transaction inside the handler, it will be done automatically.
		/// Since the <paramref name="handler"/> can run more than once, and that there is no guarantee that the transaction commits once it returns, you MAY NOT mutate any global state (counters, cache, global dictionary) inside this lambda!
		/// You must wait for the Task to complete successfully before updating the global state of the application.
		/// </remarks>
		public static async Task<TResult> ReadWriteAsync<TResult>(
			[NotNull] this IFdbDatabaseScopeProvider provider,
			[NotNull, InstantHandle] Func<IFdbTransaction, Task<TResult>> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			return await db.ReadWriteAsync(handler, ct).ConfigureAwait(false);
		}

		/// <summary>Runs a query inside a read-only transaction context, with retry-logic.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Lambda function that returns an async enumerable. The function may be called multiple times if the transaction conflicts.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <returns>Task returning the list of all the elements of the async enumerable returned by the last successful call to <paramref name="handler"/>.</returns>
		[ItemNotNull]
		public static async Task<List<TResult>> QueryAsync<TResult>(
			[NotNull] this IFdbDatabaseScopeProvider provider,
			Func<IFdbReadOnlyTransaction, Doxense.Linq.IAsyncEnumerable<TResult>> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			return await db.QueryAsync(handler, ct).ConfigureAwait(false);
		}

		/// <summary>Run an idempotent transaction block inside a write-only transaction, which can be executed more than once if any retryable error occurs.</summary>
		/// <param name="provider">Provider of the database</param>
		/// <param name="handler">Idempotent handler that should only call write methods on the transaction, and may be retried until the transaction commits, or a non-recoverable error occurs.</param>
		/// <param name="ct">Token used to cancel the operation</param>
		/// <remarks>
		/// You do not need to commit the transaction inside the handler, it will be done automatically.
		/// Since the <paramref name="handler"/> can run more than once, and that there is no guarantee that the transaction commits once it returns, you MAY NOT mutate any global state (counters, cache, global dictionary) inside this lambda!
		/// You must wait for the Task to complete successfully before updating the global state of the application.
		/// </remarks>
		public static async Task WriteAsync(
			[NotNull] this IFdbDatabaseScopeProvider provider,
			[NotNull, InstantHandle] Action<IFdbTransaction> handler,
			CancellationToken ct)
		{
			var db = await provider.GetDatabase(ct).ConfigureAwait(false);
			await db.WriteAsync(handler, ct).ConfigureAwait(false);
		}

		#endregion

	}
}