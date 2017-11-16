namespace GraphDbExamples
{
    public class Person
    {
        #region Public Properties

        public int age { get; set; } // ВІК отримуєм та витаскуєм

        public int born { get; set; }  //РІК НАРОДЖЕННЯ отримуєм та витаскуєм

        public string index { get; set; } // INDEX отримуєм та витаскуєм

        public string lastName { get; set; } // Прізвиище

        public string name { get; set; } // ІМЯ

        #endregion
    }

    public class Crew // імя та роль члена команди
    {
        #region Public Properties

        public string Name { get; set; }

        public string Role { get; set; }

        #endregion
    }

    public class Director // народився та ім'я
    {
        #region Public Properties

        public int born { get; set; }

        public string name { get; set; }

        #endregion
    }

    public class Reviewer // доповідач
    {
        #region Public Properties

        public int born { get; set; }

        public string name { get; set; }

        #endregion
    }

    public class Author // автор
    {
        #region Public Properties

        public int born { get; set; }

        public string name { get; set; }

        #endregion
    }

    public class Actor // актор
    {
        #region Public Properties

        public int born { get; set; }

        public string name { get; set; }

        #endregion
    }

    public class Movie // кіно
    {
        //must be a property with get; set;

        #region Public Properties

        public string index { get; set; }

        public int released { get; set; } // випущений

        public string title { get; set; }

        #endregion
    }
}