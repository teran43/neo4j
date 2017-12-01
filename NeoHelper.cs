namespace GraphDbExamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Neo4jClient;
    using Neo4jClient.SchemaManager;

  

    public class NeoTestHelper
    {
        #region Public Methods and Operators

        public void BuildGraph(IGraphClient graphClient)
        {
           graphClient.Cypher.Create(this.GetTestGraphCreate()).ExecuteWithoutResults();
        }

        public void DeleteGraph(IGraphClient graphClient)
        {
            graphClient.Cypher.Match("(n)").OptionalMatch("(n)-[r]-()").Delete("r,n").ExecuteWithoutResults();
        }

        public string GetTestGraphCreate()
        {
            const string testGraph =
                @"(TheDevilsAdvocate:Movie {title:""The Devil's Advocate"", released:1997,ref:'M1', tagline:'Evil has its winning ways'})
CREATE (Charlize:Person {name:'Charlize Theron', born:1975,index:'P1'})
CREATE (Al:Person {name:'Al Pacino', born:1940,index:'P2'})
CREATE (Taylor:Person {name:'Taylor Hackford', born:1944,index:'P3'})
CREATE (Keanu:Person {name:'Keanu Reeves', born:1964,index:'P4'})
CREATE
  (Keanu)-[:ACTED_IN {roles:['Kevin Lomax'],index:'R1'}]->(TheDevilsAdvocate),
  (Charlize)-[:ACTED_IN {roles:['Mary Ann Lomax'],index:'R2'}]->(TheDevilsAdvocate),
  (Al)-[:ACTED_IN {roles:['John Milton'],index:'R3'}]->(TheDevilsAdvocate),
  (Taylor)-[:DIRECTED {index:'R4'}]->(TheDevilsAdvocate)
CREATE (YouveGotMail:Movie {title:""You've Got Mail"", released:1998,ref:'M2', tagline:'At odds in life... in love on-line.'})
CREATE (TomH:Person {name:'Tom Hanks', born:1956,index:'P5'})
CREATE (NoraE:Person {name:'Nora Ephron', born:1941,index:'P6'})
CREATE (GregK:Person {name:'Greg Kinnear', born:1963,index:'P7'})
CREATE (MegR:Person {name:'Meg Ryan', born:1961,index:'P8'})
CREATE
  (TomH)-[:ACTED_IN {roles:['Joe Fox'],index:'R5'}]->(YouveGotMail),
  (MegR)-[:ACTED_IN {roles:['Kathleen Kelly'],index:'R6'}]->(YouveGotMail),
  (GregK)-[:ACTED_IN {roles:['Frank Navasky'],index:'R7'}]->(YouveGotMail),
  (NoraE)-[:DIRECTED{index:'R8'}]->(YouveGotMail)
CREATE (SleeplessInSeattle:Movie {title:'Sleepless in Seattle', released:1993,ref:'M3', tagline:'What if someone you never met, someone you never saw, 
someone you never knew was the only someone for you?'})
CREATE
  (TomH)-[:ACTED_IN {roles:['Sam Baldwin'],index:'R9'}]->(SleeplessInSeattle),
  (MegR)-[:ACTED_IN {roles:['Annie Reed'],index:'R10'}]->(SleeplessInSeattle),
  (NoraE)-[:DIRECTED{index:'R11'}]->(SleeplessInSeattle)
CREATE (WhenHarryMetSally:Movie {title:'When Harry Met Sally', released:1998,ref:'M4', tagline:'At odds in life... in love on-line.'})
CREATE (BillyC:Person {name:'Billy Crystal', born:1948,index:'P9'})
CREATE (CarrieF:Person {name:'Carrie Fisher', born:1956,index:'P10'})
CREATE (BrunoK:Person {name:'Bruno Kirby', born:1949,index:'P11'})
CREATE (RobR:Person {name:'Rob Reiner', born:1947,index:'P12'})
CREATE
  (BillyC)-[:ACTED_IN {roles:['Harry Burns'],index:'R12'}]->(WhenHarryMetSally),
  (MegR)-[:ACTED_IN {roles:['Sally Albright'],index:'R13'}]->(WhenHarryMetSally),
  (CarrieF)-[:ACTED_IN {roles:['Marie'],index:'R14'}]->(WhenHarryMetSally),
  (BrunoK)-[:ACTED_IN {roles:['Jess'],index:'R15'}]->(WhenHarryMetSally),
  (RobR)-[:DIRECTED{index:'R16'}]->(WhenHarryMetSally),
  (RobR)-[:PRODUCED{index:'R17'}]->(WhenHarryMetSally),
  (NoraE)-[:PRODUCED{index:'R18'}]->(WhenHarryMetSally),
  (NoraE)-[:WROTE{index:'R19'}]->(WhenHarryMetSally)
CREATE (ThatThingYouDo:Movie {title:'That Thing You Do', released:1996,ref:'M5', tagline:'In every life there comes a time when that thing you dream becomes 
that thing you do'})
CREATE (LivT:Person {name:'Liv Tyler', born:1977,index:'P13'})
CREATE
  (TomH)-[:ACTED_IN {roles:['Mr. White'],index:'R20'}]->(ThatThingYouDo),
  (LivT)-[:ACTED_IN {roles:['Faye Dolan'],index:'R21'}]->(ThatThingYouDo),
  (Charlize)-[:ACTED_IN {roles:['Tina'],index:'R22'}]->(ThatThingYouDo),
  (TomH)-[:DIRECTED{index:'R23'}]->(ThatThingYouDo)
";
            return testGraph;
        }

        public void WriteAllIndexDescriptionsToConsole(IGraphClient graphClient) // Написати всі індекси опису до консолі
        {
            List<IIndexMetadata> indexes = graphClient.ListAllIndexes();
            Console.WriteLine("\r\n Граф має наступні обмеження та індекси");
            foreach (IIndexMetadata indexDes in indexes)
            {
                Console.WriteLine(indexDes.ToString());
            }
            if (indexes.Count == 0)
            {
                Console.WriteLine("Нічого не знайдено");
            }
        }

        public void WriteGraphStats(IGraphClient graphClient, Neo4JClientDal neo4jClientDal)
        {
            long nodeCount = neo4jClientDal.GetNodeCount();
            long relationshipCount = neo4jClientDal.GetRelationshipCount();
            long peopleCount = neo4jClientDal.GetPersonCount();
            long movieCount = neo4jClientDal.GetMoviesCount();
            long actorCount = graphClient.Cypher.Match("(a:Actor)").Return(a => a.Count()).Results.Single();
            long producerCount = graphClient.Cypher.Match("(p:Producer)").Return(p => p.Count()).Results.Single();
            long writerCount = graphClient.Cypher.Match("(w:Writer)").Return(w => w.Count()).Results.Single();
            long directorCount = graphClient.Cypher.Match("(d:Director)").Return(d => d.Count()).Results.Single();
            long reviewerCount = graphClient.Cypher.Match("(r:Reviewer)").Return(r => r.Count()).Results.Single();
            Console.WriteLine(
                "\r\nСтатистика графу\r\nNodes={0} Relationships={1} Persons={2} Movies={3} Actors={4}\r\n"
                + "Directors={5} Writers={6} Producers{7} Reviewers={8}",
                nodeCount,
                relationshipCount,
                peopleCount,
                movieCount,
                actorCount,
                directorCount,
                writerCount,
                producerCount,
                reviewerCount);
        }

        #endregion
    }
}