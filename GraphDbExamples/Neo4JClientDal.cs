namespace GraphDbExamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;

    using Neo4jClient;
    using Neo4jClient.Cypher;

    public class Neo4JClientDal
    {
        #region Constants and Fields

        private readonly IGraphClient graphClient;

        #endregion

        #region Constructors and Destructors

        public Neo4JClientDal(IGraphClient graphClient)
        {
            this.graphClient = graphClient;
        }

        #endregion

        #region Public Methods and Operators

        public void AddActorLabels()
        {
            this.graphClient.Cypher.Match("(person:Person)-[:ACTED_IN]->(m)")
                .Set("person :Actor")
                .ExecuteWithoutResults();
        }

        public void CreateIndexWithUniqueConstraint(string label, string property)
        {
            string typeParam = "c:" + label;
            string typePropertyParam = "c." + property;
            this.graphClient.Cypher.CreateUniqueConstraint(typeParam, typePropertyParam).ExecuteWithoutResults();
               
        }

        public void CreatePerson_ACTEDIN_Movie(Person person, ActedIn actedIn, Movie movie)
        {
            this.graphClient.Cypher.Merge("(p:Person:Actor { name: {name}, born:{born} })")
                .OnCreate()
                .Set("p = {person}")
                .Set("p :Actor")
                .WithParams(new { person.name, person.born, person })
                .Merge("(m:Movie { title: {title}, released:{released} })")
                .OnCreate()
                .Set("m = {movie}")
                .WithParams(new { movie.title, movie.released, movie })
                .Merge("(p)-[r:ACTED_IN ]->(m)")
                .OnCreate()
                .Set("r = {actedIn}")
                .WithParam("actedIn", actedIn)
                .ExecuteWithoutResults();
        }

        public void CreatePerson_DIRECTED_Movie(Person person, Movie movie)
        {
            this.graphClient.Cypher.Merge("(p:Person { name: {name}, born:{born} })")
                .OnCreate()
                .Set("p = {person}")
                .Set("p :Director")
                .WithParams(new { person.name, person.born, person })
                .Merge("(m:Movie { title: {title}, released:{released} })")
                .OnCreate()
                .Set("m = {movie}")
                .WithParams(new { movie.title, movie.released, movie })
                .Merge("(p)-[:DIRECTED]->(m)")
                .ExecuteWithoutResults();
        }

        public void CreatePerson_PRODUCED_Movie(Person person, Movie movie)
        {
            this.graphClient.Cypher.Merge("(p:Person { name: {name}, born:{born} })")
                .OnCreate()
                .Set("p = {person}")
                .Set("p :Producer")
                .WithParams(new { person.name, person.born, person })
                .Merge("(m:Movie { title: {title}, released:{released} })")
                .OnCreate()
                .Set("m = {movie}")
                .WithParams(new { movie.title, movie.released, movie })
                .Merge("(p)-[:PRODUCED]->(m)")
                .ExecuteWithoutResults();
        }

        public void CreatePerson_REVIEWED_Movie(Person person, Reviewed reviewed, Movie movie)
        {
            this.graphClient.Cypher.Merge("(p:Person { name: {name}, born:{born} })")
                .OnCreate()
                .Set("p = {person}")
                .Set("p :Reviewer")
                .WithParams(new { person.name, person.born, person })
                .Merge("(m:Movie { title: {title}, released:{released} })")
                .OnCreate()
                .Set("m = {movie}")
                .WithParams(new { movie.title, movie.released, movie })
                .Merge("(p)-[r:REVIEWED]->(m)")
                .OnCreate()
                .Set("r = {reviewed}")
                .WithParam("reviewed", reviewed)
                .ExecuteWithoutResults();
        }

        public void CreatePerson_WROTE_Movie(Person person, Movie movie)
        {
            this.graphClient.Cypher.Merge("(p:Person { name: {name}, born:{born} })")
                .OnCreate()
                .Set("p = {person}")
                .Set("p :Writer")
                .WithParams(new { person.name, person.born, person })
                .Merge("(m:Movie { title: {title}, released:{released} })")
                .OnCreate()
                .Set("m = {movie}")
                .WithParams(new { movie.title, movie.released, movie })
                .Merge("(p)-[:WROTE]->(m)")
                .ExecuteWithoutResults();
        }

        public void DeleteACTED_INRelationship(string personName, string movieTitle)
        {
            this.graphClient.Cypher.Match("(m:Movie{title:{titleParam}})-[r:ACTED_IN]-(p:Person {name:{nameParam}})")
                .WithParams(new { titleParam = movieTitle, nameParam = personName })
                .Delete("r")
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void DeleteDIRECTEDRelationship(string personName, string movieTitle)
        {
            this.graphClient.Cypher.Match("(m:Movie{title:{titleParam}})-[r:DIRECTED]-(p:Person {name:{nameParam}})")
                .WithParams(new { titleParam = movieTitle, nameParam = personName })
                .Delete("r")
                .ExecuteWithoutResults();
        }

        public void DeleteMovie(string movieTitle)
        {
            this.graphClient.Cypher.OptionalMatch("(m:Movie{title:{titleParam}})-[r]-()")
                .WithParam("titleParam", movieTitle)
                .Delete("r,m")
                .ExecuteWithoutResults();
        }

        public void DeletePRODUCEDRelationship(string personName, string movieTitle)
        {
            this.graphClient.Cypher.Match("(m:Movie{title:{titleParam}})-[r:PRODUCED]-(p:Person {name:{nameParam}})")
                .WithParams(new { titleParam = movieTitle, nameParam = personName })
                .Delete("r")
                .ExecuteWithoutResults();
        }

        public void DeletePerson(string personName)
        {
            this.graphClient.Cypher.OptionalMatch("(person:Person)-[r]-()")
                .Where((Person person) => person.name == personName)
                .Delete("r,person")
                .ExecuteWithoutResults();
        }

        public void DeleteREVIEWEDRelationship(string personName, string movieTitle)
        {
            this.graphClient.Cypher.Match("(m:Movie{title:{titleParam}})-[r:REVIEWED]-(p:Person {name:{nameParam}})")
                .WithParams(new { titleParam = movieTitle, nameParam = personName })
                .Delete("r")
                .ExecuteWithoutResults();
        }

        public void DeleteWROTERelationship(string personName, string movieTitle)
        {
            this.graphClient.Cypher.Match("(m:Movie{title:{titleParam}})-[r:WROTE]-(p:Person {name:{nameParam}})")
                .WithParams(new { titleParam = movieTitle, nameParam = personName })
                .Delete("r")
                .ExecuteWithoutResults();
        }

        public void ExecuteScopedTransactions(List<ICypherFluentQuery> transactionList)
        {
            //need to reference System.Transactions
            using (var scope = new TransactionScope()) 
            {
                foreach (ICypherFluentQuery transaction in transactionList)
                {
                    transaction.ExecuteWithoutResults();
                }
                scope.Complete();
            }
        }

        public List<string> GetActorsCocoActors(string actorsName)
        {
            List<string> cocoActorsNames =
                this.graphClient.Cypher.Match(
                    "(actor:Person {name:{nameParam}})-[:ACTED_IN]->(m)<-[:ACTED_IN]-(coActor:Person),(coActor)-[:ACTED_IN]->(m2)<-[:ACTED_IN]-(cocoActor:Person)")
                    .WithParam("nameParam", actorsName)
                    .Where("NOT (actor)-[:ACTED_IN]->(m2)")
                    .ReturnDistinct(cocoActor => cocoActor.As<Person>().name)
                    .OrderBy("cocoActor.name")
                    .Results.ToList();
            return cocoActorsNames;
        }

        public List<string> GetActorsMovieTitles(string personName)
        {
            List<string> ActorsMovieTitles =
                this.graphClient.Cypher.Match("(actor:Person {name:{nameParam}})-[r:ACTED_IN]->(m:Movie)")
                    .WithParam("nameParam", personName)
                    .Return(m => m.As<Movie>().title)
                    .OrderBy("m.title")
                    .Results.ToList();
            return ActorsMovieTitles;
        }

        public List<string> GetActorsRolesInMovie(string actorsName, string movieTitle)
        {
            try
            {
                List<string> actorsRoles =
                    this.graphClient.Cypher.Match(
                        "(p:Person {name:{nameParam}})-[r:ACTED_IN]->(m:Movie {title:{titleParam}})")
                        .WithParams(new { nameParam = actorsName, titleParam = movieTitle })
                        .Return(r => r.As<ActedIn>().roles)
                        .Results.Single()
                        .ToList();
                return actorsRoles;
            }
            catch (InvalidOperationException) //thrown when nothing found
            {
                return new List<string>();
            }
        }

        public List<Crew> GetCrewOfMovie(string movieTitle)
        {
            List<Crew> movieCrew =
                this.graphClient.Cypher.Match("(movie:Movie {title: {titleParam}})")
                    .OptionalMatch("(person:Person)-[r:DIRECTED|:PRODUCED|:WROTE]->(movie:Movie)")
                    .WithParam("titleParam", movieTitle)
                    .Return((person, r) => new Crew { Name = person.As<Person>().name, Role = r.Type() })
                    .OrderBy("person.name")
                    .Results.ToList();

            return movieCrew;
        }

        public async Task<IEnumerable<Crew>> GetCrewOfMovieAsync(string movieTitle)
        {
            IEnumerable<Crew> movieCrew =
                await
                    this.graphClient.Cypher.Match("(movie:Movie {title: {titleParam}})")
                        .OptionalMatch("(person:Person)-[r:DIRECTED|:PRODUCED|:WROTE]->(movie:Movie)")
                        .WithParam("titleParam", movieTitle)
                        .Return((person, r) => new Crew { Name = person.As<Person>().name, Role = r.Type() })
                        .OrderBy("person.name")
                        .ResultsAsync;

            return movieCrew;
        }

        public List<string> GetDirectorsMovieTitles(string directorsName)
        {
            List<string> directorsMovies =
                this.graphClient.Cypher.Match("(p:Person {name:{nameParam}})-[:DIRECTED]->(m:Movie)")
                    .WithParam("nameParam", directorsName)
                    .Return(m => m.As<Movie>().title)
                    .OrderBy("m.title")
                    .Results.ToList();
            return directorsMovies;
        }

        public long GetMoviesCount()
        {
            long movieCount = this.graphClient.Cypher.Match("(m:Movie)").Return(m => m.Count()).Results.Single();
            return movieCount;
        }

        public long GetNodeCount()
        {
            long nodeCount = this.graphClient.Cypher.Match("(n)").Return(n => All.Count()).Results.Single();
            return nodeCount;
        }

        public Person GetPerson(string personName)
        {
            try
            {
                Person person =
                    this.graphClient.Cypher.Match("(p:Person {name: {nameParam}})")
                        .WithParam("nameParam", personName)
                        .Return(p => p.As<Person>())
                        .Results.Single();
                return person;
            }
            catch (InvalidOperationException) //thrown when nothing found
            {
                return null;
            }
        }

        public long GetPersonCount()
        {
            long personCount = this.graphClient.Cypher.Match("(p:Person)").Return(p => p.Count()).Results.Single();
            return personCount;
        }

        public long GetRelationshipCount()
        {
            long relationshipCount = this.graphClient.Cypher.Match("()-[r]->()").Return(r => r.Count()).Results.Single();
            return relationshipCount;
        }

        #endregion
    }
}