﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Azure.BlobStorage.DataStores;
using Cqrs.Azure.BlobStorage.Repositories;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;
using Cqrs.Repositories.Queries;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.BlobStorage.Test.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="TableStorageDataStore{TData}"/> class
	/// </summary>
	[TestClass]
	public class TableStorageDataStoreTests
	{
		[TestMethod]
		public void Add_ValidProjectionView_ProjectionViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ThreadedContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var dataStore = new TableStorageDataStore<TestEvent>(logger, new TableStorageDataStoreConnectionStringFactory(new ConfigurationManager(), logger));

			var event1 = new TestEvent
			{
				Rsn = Guid.NewGuid(),
				Id = Guid.NewGuid(),
				CorrelationId = correlationIdHelper.GetCorrelationId(),
				Frameworks = new List<string> { "Test 1" },
				TimeStamp = DateTimeOffset.UtcNow
			};

			// Act
			dataStore.Add(event1);

			// Assert
			var timer = new Stopwatch();
			var repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEvent>, TestEvent>(() => dataStore, null);
			timer.Start();
			TestEvent view = repository.Load(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Id, view.Id);
		}

		[TestMethod]
		public void Add_ValidProjectionEntityView_ProjectionEntityViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ThreadedContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var dataStore = new TableStorageDataStore<TestEntity>(logger, new TableStorageDataStoreConnectionStringFactory(new ConfigurationManager(), logger));

			var event1 = new TestEntity
			{
				Rsn = Guid.NewGuid(),
				Name = "Name"
			};

			// Act
			dataStore.Add(event1);

			// Assert
			var timer = new Stopwatch();
			var repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEntity>, TestEntity>(() => dataStore, null);
			timer.Start();
			TestEntity view = repository.Load(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Rsn, view.Rsn);
			Assert.AreEqual(event1.Name, view.Name);
		}

		[TestMethod]
		public void Update_ValidProjectionEntityView_ProjectionEntityViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ThreadedContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var dataStore = new TableStorageDataStore<TestEntity>(logger, new TableStorageDataStoreConnectionStringFactory(new ConfigurationManager(), logger));

			var event1 = new TestEntity
			{
				Rsn = Guid.NewGuid(),
				Name = "Name1"
			};
			dataStore.Add(event1);

			// The repo disposes the datastore, so a copy is needed.
			var repoDataStore = new TableStorageDataStore<TestEntity>(logger, new TableStorageDataStoreConnectionStringFactory(new ConfigurationManager(), logger));
			var repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEntity>, TestEntity>(() => repoDataStore, null);
			TestEntity view = repository.Load(event1.Rsn);
			view.Name = "Name2";

			// Act
			dataStore.Update(event1);

			// Assert
			var timer = new Stopwatch();
			timer.Start();
			// Refresh the data store due to disposal.
			repoDataStore = new TableStorageDataStore<TestEntity>(logger, new TableStorageDataStoreConnectionStringFactory(new ConfigurationManager(), logger));
			repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEntity>, TestEntity>(() => repoDataStore, null);
			view = repository.Load(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Rsn, view.Rsn);
			Assert.AreEqual(event1.Name, view.Name);
		}

		public class TestQueryStrategy : IQueryStrategy
		{
			#region Implementation of IQueryStrategy

			public IQueryPredicate QueryPredicate { get; set; }

			#endregion
		}

		public class TestQueryBuilder<TData> : QueryBuilder<TestQueryStrategy, TData>
			where TData : Entity
		{
			public TestQueryBuilder(IDataStore<TData> dataStore, IDependencyResolver dependencyResolver)
				: base(dataStore, dependencyResolver)
			{
			}

			#region Overrides of QueryBuilder<TestQueryStrategy,TData>

			protected override IQueryable<TData> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null)
			{
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}