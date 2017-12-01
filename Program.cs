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
                    "Неможливо підключитися до сервера \r \n Виконати підключення з браузера \r \n, щоб переконатися, що ви можете увійти",
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
                neoHelper.BuildGraph(graphClient); // побудова графу в фалі NeoHelper
                scope.Complete();
            }
            Console.WriteLine("Побудований граф");
            var neo4jClientDal = new Neo4JClientDal(graphClient);
            neoHelper.WriteGraphStats(graphClient, neo4jClientDal); // статистика графу
            neoHelper.WriteAllIndexDescriptionsToConsole(graphClient);

            Console.WriteLine("\r\n Додавання міток до графу.");
            var transactionList = new List<ICypherFluentQuery>
            {
                graphClient.Cypher.Match("(person:Person)-[:ACTED_IN]->(m)").Set("person :Actor"),
                graphClient.Cypher.Match("(person:Person)-[:PRODUCED]->(m)").Set("person :Producer"),
                graphClient.Cypher.Match("(person:Person)-[:DIRECTED]->(m)").Set("person :Director"),
                graphClient.Cypher.Match("(person:Person)-[:WROTE]->(m)").Set("person :Writer"),
                graphClient.Cypher.Match("(person:Person)-[:REVIEWED]->(m)").Set("person :Reviewer")
            };
            neo4jClientDal.ExecuteScopedTransactions(transactionList);

            Console.WriteLine("\r\n Створення певного індексу на Actor.name властивості.");
            neo4jClientDal.CreateIndexWithUniqueConstraint("Actor", "name");
            Console.WriteLine("Створення індексу у Movie.title властивості.");
            graphClient.Cypher.Create("INDEX ON :Movie(title)").ExecuteWithoutResults();
            //Some indexes take time to build so they may not be immediately onLine 

            Console.WriteLine("\r\n Додавання Daniel Craig і Skyfall до графу"); // Додавання Daniel Craig і Skyfall до графу
            var danielCraig = new Person { born = 1968, name = "Daniel Craig" }; //Актор
            var skyfall = new Movie { released = 2012, title = "Skyfall" }; // Фільм
            var actedIn = new ActedIn { roles = new[] { "James Bond" } }; //Роль
            neo4jClientDal.CreatePerson_ACTEDIN_Movie(danielCraig, actedIn, skyfall); // відправили

            Console.WriteLine("\n Додання критик,письменник,директор,продюсер для Skyfall ");
            var davidJones = new Person { born = 1968, name = "David Jones" };
            var skyFallReview = new Reviewed { rating = 74, summary = "Відмінно, але доволі насильницький" };
            neo4jClientDal.CreatePerson_REVIEWED_Movie(davidJones, skyFallReview, skyfall);
            var robertWade = new Person { born = 1962, name = "Robert Wade" };
            neo4jClientDal.CreatePerson_WROTE_Movie(robertWade, skyfall);
            var samMendes = new Person { born = 1965, name = "Sam Mendes" };
            neo4jClientDal.CreatePerson_DIRECTED_Movie(samMendes, skyfall);
            var barbaraBroccoli = new Person { born = 1960, name = "Barbara Broccoli" };
            neo4jClientDal.CreatePerson_PRODUCED_Movie(barbaraBroccoli, skyfall);
            Console.WriteLine("Готово\r\n");
           

            neoHelper.WriteAllIndexDescriptionsToConsole(graphClient);

            neoHelper.WriteGraphStats(graphClient, neo4jClientDal);
            bool _swich = true;

            while (_swich)
            {
                Console.WriteLine("\n Натисніть відповідну цифру \n Меню \n 1.Ввести запит. \n 2.Вихід \r\n");
                int num = Convert.ToInt32(Console.ReadLine());
                switch (num)
                {
                    case 1:
                        {
                            Console.WriteLine("Меню \n 1.Вибірка фільмів по актору. \n 2.Вибірка акторів по кіну \n 3.Видалити актора з БД \n 4.Вихід \r\n");
                            int num1 = Convert.ToInt32(Console.ReadLine());
                            switch (num1)
                            {
                                case 1:
                                    {
                                        Console.WriteLine("\n Введіть ім'я та прізвище актора \r\n");
                                        string nameActor = Console.ReadLine();
                                        List<string> MoviesActor = neo4jClientDal.GetActorsMovieTitles(nameActor);
                                        Console.WriteLine("\n Кіна: \n");
                                        foreach (string movie in MoviesActor)
                                        {
                                            Console.WriteLine(movie);
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        Console.WriteLine("\n Введіть назву кіна\r\n");
                                        string nameFilm = Console.ReadLine();
                                        Task<IEnumerable<Crew>> getcrewofMovieTask2 = neo4jClientDal.GetCrewOfMovieAsync(nameFilm);
                                        List<Crew> crewMembers2 = getcrewofMovieTask2.Result.ToList();
                                        foreach (Crew crewMember in crewMembers2)
                                        {
                                            Console.WriteLine("{0}\t Роль: {1}", crewMember.Name, crewMember.Role);
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        Console.WriteLine("\n Введіть ім'я та прізвище актора \n ");
                                        string nameActor = Console.ReadLine();
                                        neo4jClientDal.DeletePerson(nameActor);
                                        Console.WriteLine("Готово!");
                                        break;
                                    }
                                case 4:
                                    {
                                        Console.WriteLine("\nВихід\n");
                                        break;
                                    }
                            }
                            break;
                        }
                    case 2:
                        {
                            goto exit;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("\n Перевірте запис");
                            break;
                        }
                }

            }
            Console.WriteLine();
            exit:
            Console.WriteLine("\r\n Будь ласка натисніть Enter для виходу");
            Console.ReadLine();
        }

        #endregion
    }
}