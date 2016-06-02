using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.IO;

namespace AdviceBotDocumentDB
{
    class Program
    {
        private const string EndpointUri = "https://advicebotdb.documents.azure.com:443/";
        private const string PrimaryKey = "DIjubQfc8c96jrUPnmxTDRfe2jcG6iJrXtZZ0XzyGem3hAGmCYu8GEj0NAiYN1mWIZPsh4RAMuoTDShxsx0cMg==";
        private DocumentClient client;

        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.AdviceBotDBOps().Wait();
                p.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }
        private async Task AdviceBotDBOps()
        {
            this.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            //Optional:  Delete the old database
            await this.client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("AdviceBotDB"));

            //Create the database if it does not exist
            await this.CreateDatabaseIfNotExists("AdviceBotDB");

            //Create the document collection if it does not exist
            await this.CreateDocumentCollectionIfNotExists("AdviceBotDB", "AdviceCollection");

            //TODO:  Eventually, for each item in a csv, read the item and create documents
            await this.CreateAdviceDocuments();

            ////Create the document if it does not exist
            //await this.CreateAdviceDocumentIfNotExists("AdviceBotDB", "AdviceCollection", CreateAdvice("1", "Test Advice Item"));

            ////Create the document if it does not exist
            //await this.CreateAdviceDocumentIfNotExists("AdviceBotDB", "AdviceCollection", CreateAdvice("2", "Another Advice Item"));

            //Execute a diagnostic query
            this.ExecuteSimpleQuery("AdviceBotDB", "AdviceCollection", "Test Advice Item");
        }

        private async Task CreateAdviceDocuments()
        {
            var reader = new StreamReader(File.OpenRead(@"../../AdviceForLife.csv"));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');


                //Let everyone know that we have an item
                Console.WriteLine("Creating Document:  " + line);

                //create the item
                await this.CreateAdviceDocumentIfNotExists("AdviceBotDB", "AdviceCollection", CreateAdvice(values[0], values[1], values[2]));               

            }
        }

        public class Advice
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string AdviceTitle { get; set; }
            public string AdviceTags { get; set; }
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        private async Task CreateDatabaseIfNotExists(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
                this.WriteToConsoleAndPromptToContinue("Found {0}", databaseName);
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                    this.WriteToConsoleAndPromptToContinue("Created {0}", databaseName);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
                this.WriteToConsoleAndPromptToContinue("Found {0}", collectionName);
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        new DocumentCollection { Id = collectionName },
                        new RequestOptions { OfferThroughput = 400 });

                    this.WriteToConsoleAndPromptToContinue("Created {0}", collectionName);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateAdviceDocumentIfNotExists(string databaseName, string collectionName, Advice advice)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, advice.Id));
                //this.WriteToConsoleAndPromptToContinue("Found {0}", advice.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), advice);
                    //this.WriteToConsoleAndPromptToContinue("Created Advice {0}", advice.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        private Advice CreateAdvice(string id, string adviceTitle, string adviceTags)
        {
            Advice adviceItem = new Advice
            {
                Id = id,
                AdviceTitle = adviceTitle,
                AdviceTags = adviceTags
            };

            return adviceItem;
        }

        private void ExecuteSimpleQuery(string databaseName, string collectionName, string query)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Here we find the Andersen family via its LastName
            IQueryable<Advice> adviceQuery = this.client.CreateDocumentQuery<Advice>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(f => f.AdviceTitle == query);

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            Console.WriteLine("Running LINQ query...");
            foreach (Advice family in adviceQuery)
            {
                Console.WriteLine("\tRead {0}", family);
            }

            // Now execute the same query via direct SQL
            IQueryable<Advice> familyQueryInSql = this.client.CreateDocumentQuery<Advice>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    "SELECT * FROM Advice WHERE Family.lastName = '" + query + "'",
                    queryOptions);

            Console.WriteLine("Running direct SQL query...");
            foreach (Advice family in adviceQuery)
            {
                Console.WriteLine("\tRead {0}", family);
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
