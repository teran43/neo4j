
namespace GraphDbExamples
{
    public class ActedIn //Виступив в
    {
        #region Public Properties

        public string[] roles { get; set; }
        public string index { get; set; }

        #endregion
    }

    public class Reviewed // переглянуто
    {
        //must be a property with get; set;

        #region Public Properties

        public int rating { get; set; } // рейтинг

        public string summary { get; set; } // підсумок

        #endregion
    }

    public class Directed
    {
    }

    public class Produced
    {
    }

}
