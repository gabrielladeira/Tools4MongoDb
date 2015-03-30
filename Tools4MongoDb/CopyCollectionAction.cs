using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Tools4MongoDb
{
    public class CopyCollectionAction
    {
        public void Run(CopyCollectionArgs args)
        {
            Console.WriteLine();
            Console.WriteLine("Source: {0} - {1} - {2}", args.SourceUrl, args.SourceDatabase, args.SourceCollection);
            var sourceCollection = MongoHelper.GetCollection(
                url: args.SourceUrl,
                databaseName: args.SourceDatabase,
                collectionName: args.SourceCollection);

            args.DestinyUrl = args.DestinyUrl ?? args.SourceUrl;
            args.DestinyDatabase = args.DestinyDatabase ?? args.SourceDatabase;
            Console.WriteLine();
            Console.WriteLine("Destiny: {0} - {1} - {2}", args.DestinyUrl, args.DestinyDatabase, args.DestinyCollection);
            var destinyCollection = MongoHelper.GetCollection(
                url: args.DestinyUrl,
                databaseName: args.DestinyDatabase,
                collectionName: args.DestinyCollection);


            if (args.ClearDestiny)
            {
                Console.WriteLine();
                Console.Write("Cleaning Destiny Collection");
                destinyCollection.RemoveAll();
            }

            var sourceCollectionCount = sourceCollection.Count();
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Founded {0:##,###} documents", sourceCollectionCount);

            long maxDocuments;
            long skipDocuments;
            CalcSkipAndMaxDocuments(args, sourceCollectionCount, out maxDocuments, out skipDocuments);

            Console.WriteLine();
            Console.WriteLine();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var cursor = sourceCollection.FindAll();
            cursor.SetFlags(QueryFlags.NoCursorTimeout);

            var destinyCollectionCount = CopyDocuments(args, cursor, skipDocuments, maxDocuments, destinyCollection);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine(
                "Copyed {0} documents in {1} = {2}",
                destinyCollectionCount,
                stopwatch.Elapsed,
                new TimeSpan(stopwatch.ElapsedTicks / destinyCollectionCount));
        }

        private static int CopyDocuments(
            CopyCollectionArgs args,
            IEnumerable<BsonDocument> cursor,
            long skipDocuments,
            long maxDocuments,
            MongoCollection<BsonDocument> destinyCollection)
        {
            var counter = -1;
            var breakLine = false;
            var destinyCollectionCount = -1;
            double lastPrintedPercentual = -1;
            foreach (var item in cursor.AsEnumerable())
            {
                counter += 1;
                if (counter % skipDocuments != 0)
                {
                    if (counter % 50 == 0)
                    {
                        Console.Write(".");
                        breakLine = true;
                    }

                    continue;
                }

                destinyCollectionCount += 1;

                var percentual = (double)destinyCollectionCount / maxDocuments;
                if (percentual.ToString("p") != lastPrintedPercentual.ToString("p"))
                {
                    if (breakLine)
                    {
                        Console.WriteLine();
                        breakLine = false;
                    }

                    Console.WriteLine("{0:g} -> {1:##,##0}/{2:##,##0} = {3:p}", DateTime.Now, destinyCollectionCount,
                        maxDocuments, percentual);
                    lastPrintedPercentual = percentual;
                }

                try
                {
                    destinyCollection.Insert(item);
                }
                catch (MongoDuplicateKeyException)
                {
                    switch (args.OnDuplicateKey)
                    {
                        case DuplicateKey.Skip:
                            Console.WriteLine("\tSkiped because already exists ({0})", item["_id"]);
                            break;

                        case DuplicateKey.Abort:
                            Console.WriteLine("\tAborting because duplicateKey ({0})", item["_id"]);
                            throw;

                        case DuplicateKey.GenerateNewKey:
                            Console.WriteLine("\tGenerate new key ({0})", item["_id"]);
                            item["_id"] = BsonNull.Value;
                            try
                            {
                                destinyCollection.Insert(item);
                            }
                            catch (MongoDuplicateKeyException)
                            {
                                Console.WriteLine("\t\tSkiped on second try ({0})", item["_id"]);
                            }
                            break;
                    }
                }

                if (destinyCollectionCount >= maxDocuments)
                {
                    break;
                }
            }
            return destinyCollectionCount;
        }

        private static void CalcSkipAndMaxDocuments(CopyCollectionArgs args, long sourceCollectionCount, out long maxDocuments, out long skipDocuments)
        {
            maxDocuments = sourceCollectionCount;
            skipDocuments = 1;

            if (args.SamplingPercentualDocuments.HasValue)
            {
                maxDocuments = (long)Math.Ceiling(sourceCollectionCount * args.SamplingPercentualDocuments.Value);
                skipDocuments = sourceCollectionCount / maxDocuments;
                Console.WriteLine();
                Console.Write(
                    "Sampling to 1 by {0} documents = {1:p} = {2}",
                    skipDocuments,
                    args.SamplingPercentualDocuments.Value,
                    maxDocuments);
                Console.WriteLine();
            }
            else if (args.SamplingMaxDocuments.HasValue)
            {
                maxDocuments = args.SamplingMaxDocuments.Value;
                skipDocuments = sourceCollectionCount / maxDocuments;
                Console.WriteLine();
                Console.Write(
                    "Sampling to 1 by {0} documents = {1:##,###} documents",
                    skipDocuments,
                    args.SamplingMaxDocuments.Value);
                Console.WriteLine();
            }

            if (args.MaxDocuments.HasValue &&
                maxDocuments > args.MaxDocuments.Value)
            {
                Console.WriteLine();
                Console.Write("Limiting to {0:##,###} documents", args.MaxDocuments.Value);
                maxDocuments = args.MaxDocuments.Value;
            }
        }
    }
}