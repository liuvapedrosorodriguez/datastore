﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataStore.Impl.DocumentDb;
using DataStore.Impl.DocumentDb.Config;
using DataStore.Interfaces.LowLevel;
using DataStore.MessageAggregator;
using DataStore.Models.Messages;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.Tests.TestHarness
{
    public class DocumentDbTestHarness : ITestHarness
    {
        private readonly DocumentDbRepository documentDbRepository;
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();
        private readonly DocumentDbSettings settings;

        private DocumentDbTestHarness(DocumentDbSettings settings)
        {
            this.settings = settings;
            documentDbRepository = new DocumentDbRepository(settings);
            DataStore = new DataStore(documentDbRepository, messageAggregator);
        }

        public DataStore DataStore { get; }

        public List<IMessage> AllMessages => messageAggregator.AllMessages.ToList();

        #region

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate, documentDbRepository,
                messageAggregator);
            newAggregate.CommitClosure().Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new()
        {
            var query = extendQueryable == null
                ? documentDbRepository.CreateDocumentQuery<T>()
                : extendQueryable(documentDbRepository.CreateDocumentQuery<T>());
            return documentDbRepository
                .ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable()))
                .Result;
        }

        #endregion

        public static ITestHarness Create(DocumentDbSettings dbConfig)
        {
            ClearTestDatabase(dbConfig);

            return new DocumentDbTestHarness(dbConfig);
        }

        public async void RemoveAllCollections()
        {
            DocumentClient client;

            GetDocumentClient(settings, out client);
            var db = (await client.ReadDatabaseFeedAsync().ConfigureAwait(false)).Single(d => d.Id == settings.DatabaseName);

            var collections = await client.ReadDocumentCollectionFeedAsync(db.CollectionsLink).ConfigureAwait(false);
            foreach (var documentCollection in collections)
                await client.DeleteDocumentCollectionAsync(documentCollection.SelfLink).ConfigureAwait(false);
        }

        private static void ClearTestDatabase(DocumentDbSettings documentDbSettings)
        {
            DocumentClient client;
            DocumentCollection collection;

            GetDocumentClient(documentDbSettings, out client);

            GetDocumentCollection(documentDbSettings, client, out collection);

            if (collection != null)
                DeleteAllDocsInCollection(client, collection);
        }

        private static void DeleteAllDocsInCollection(DocumentClient documentClient, DocumentCollection collection)
        {
            var allDocsInCollection = documentClient.CreateDocumentQuery(collection.DocumentsLink,
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true,
                        MaxDegreeOfParallelism = -1,
                        MaxBufferedItemCount = -1
                    })
                .ToList();

            foreach (var doc in allDocsInCollection)
            {
                //required if collection is partitioned
                var requestOptions = new RequestOptions();

                if (collection.PartitionKey.Paths.Count > 0)
                {
                    var key = collection.PartitionKey.Paths[0];
                    PartitionKey partitionKey;
                    //NOTE: these values will always be cahnged to lowercase by docdb so must make them lowercase on model
                    if (key == "/schema") partitionKey = new PartitionKey(((dynamic) doc).schema);
                    else if (key == "/id") partitionKey = new PartitionKey(doc.Id);
                    else throw new Exception("Error locating partition key");
                    requestOptions.PartitionKey = partitionKey;
                }

                documentClient.DeleteDocumentAsync(doc.SelfLink, requestOptions).Wait();
            }
        }

        private static void GetDocumentCollection(DocumentDbSettings documentDbSettings, DocumentClient documentClient,
            out DocumentCollection collection)
        {
            collection =
                documentClient.ReadDocumentCollectionAsync(documentDbSettings.CollectionSelfLink()).Result;
        }

        private static void GetDocumentClient(DocumentDbSettings documentDbSettings, out DocumentClient documentClient)
        {
            documentClient = new DocumentDbClientFactory(documentDbSettings).GetDocumentClient();
        }
    }
}