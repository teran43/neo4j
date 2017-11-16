namespace GraphDbExamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Windows.Forms;

    using Neo4jClient;
    using Neo4jClient.Cypher;
    using Neo4jClient.SchemaManager;

    internal class Program
    {
        #region Methods

        private static void Main()
        {
            DialogResult result = MessageBox.Show(
                "Необхідно видалити будь-який існуючий Граф та створити новий граф",
                "УВАГА!",
                MessageBoxButtons.OKCancel);
            if (result == DialogResult.Cancel)
            {
                throw new Exception("Користувач скасував тест");
            }

            //переконайтеся, що Neo4J двіжок працює перед відкриттям db
            //і що "мій пароль" є коректним для успішного переходу до Uri
            IGraphClient graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "vldsdvskj");  // вказали логін пароль та посилання
            try
            {
                graphClient.Connect(); // законектились в результаті помилки вивели смс та вийшли
            }
            catch (AggregateException) // якщо не запущений двіжок то зайдем в ексепшен
            {
                MessageBox.Show(
                    "Неможливо підключитися до сервера \r \nВиконати підключення з браузера \r \n, щоб переконатися, що ви можете увійти",
                    "Помилка!",
                    MessageBoxButtons.OK);
                Application.Exit(); 
            }
            var neoHelper = new NeoTestHelper(); // створили об'єкт класу NeoTestHelper, щоб могли доступитися до методів в ньому
            Console.WriteLine("Створення графу, будь ласка, зачекайте.");
            neoHelper.DeleteGraph(graphClient); // передали об'єкт graphClient в метод DeleteGraph об'єкту який знаходиться neoHelper
            graphClient.DropAllIndexes();
            using (var scope = new TransactionScope()) // using System.Transactions;
            {
                neoHelper.BuildGraph(graphClient);
                scope.Complete();
            }
            Console.WriteLine("Built test graph.");
            var neo4jClientDal = new Neo4JClientDal(graphClient);
            neoHelper.WriteGraphStats(graphClient, neo4jClientDal);
            neoHelper.WriteAllIndexDescriptionsToConsole(graphClient);

            Console.WriteLine("\r\nAdding  Labels to the  graph.");
            var transactionList = new List<ICypherFluentQuery>
            {
                graphClient.Cypher.Match("(person:Person)-[:ACTED_IN]->(m)").Set("person :Actor"),
                graphClient.Cypher.Match("(person:Person)-[:PRODUCED]->(m)").Set("person :Producer"),
                graphClient.Cypher.Match("(person:Person)-[:DIRECTED]->(m)").Set("person :Director"),
                graphClient.Cypher.Match("(person:Person)-[:WROTE]->(m)").Set("person :Writer"),
                graphClient.Cypher.Match("(person:Person)-[:REVIEWED]->(m)").Set("person :Reviewer")
            };
            neo4jClientDal.ExecuteScopedTransactions(transactionList);

            Console.WriteLine("\r\nCreating distinct index on the Actor.name property.");
            neo4jClientDal.CreateIndexWithUniqueConstraint("Actor", "name");
            Console.WriteLine("Creating  index on the Movie.title property.");
            graphClient.Cypher.Create("INDEX ON :Movie(title)").ExecuteWithoutResults();
            //Some indexes take time to build so they may not be immediately onLine 

            Console.WriteLine("\r\nAdding Daniel Craig and Skyfall to the graph");
            var danielCraig = new Person { born = 1968, name = "Daniel Craig" };
            var skyfall = new Movie { released = 2012, title = "Skyfall" };
            var actedIn = new ActedIn { roles = new[] { "James Bond" } };
            neo4jClientDal.CreatePerson_ACTEDIN_Movie(danielCraig, actedIn, skyfall);

            Console.WriteLine("Adding reviewer,writer,director,producer for Skyfall ");
            var davidJones = new Person { born = 1968, name = "David Jones" };
            var skyFallReview = new Reviewed { rating = 74, summary = "Excellent but gratuitously violent" };
            neo4jClientDal.CreatePerson_REVIEWED_Movie(davidJones, skyFallReview, skyfall);
            var robertWade = new Person { born = 1962, name = "Robert Wade" };
            neo4jClientDal.CreatePerson_WROTE_Movie(robertWade, skyfall);
            var samMendes = new Person { born = 1965, name = "Sam Mendes" };
            neo4jClientDal.CreatePerson_DIRECTED_Movie(samMendes, skyfall);
            var barbaraBroccoli = new Person { born = 1960, name = "Barbara Broccoli" };
            neo4jClientDal.CreatePerson_PRODUCED_Movie(barbaraBroccoli, skyfall);
            Console.WriteLine("Done\r\n\r\nListing Tom Hanks' Movies\r\n");
            List<string> TomsMovies = neo4jClientDal.GetActorsMovieTitles("Tom Hanks");

            foreach (string movie in TomsMovies)
            {
                Console.WriteLine(movie);
            }

            List<string> cocoActors = neo4jClientDal.GetActorsCocoActors("Tom Hanks");
            Console.WriteLine(
                "\r\nActors who have not worked with Tom Hanks\r\nbut have worked with actors who have worked with Tom\r\n");
            foreach (string cocoActor in cocoActors)
            {
                Console.WriteLine(cocoActor);
            }

            Console.WriteLine("\r\nGetting the crew of When Harry Met Sally asynchronously\r\n");
            Task<IEnumerable<Crew>> getcrewofMovieTask = neo4jClientDal.GetCrewOfMovieAsync("When Harry Met Sally");
            List<Crew> crewMembers = getcrewofMovieTask.Result.ToList();
            foreach (Crew crewMember in crewMembers)
            {
                Console.WriteLine("{0}\t Role {1}", crewMember.Name, crewMember.Role);
            }
            neoHelper.WriteAllIndexDescriptionsToConsole(graphClient);

            neoHelper.WriteGraphStats(graphClient, neo4jClientDal);
            Console.WriteLine("\r\nPlease hit 'return' to exit");
            Console.ReadLine();
        }

        #endregion
    }
}