using System.Collections.Generic;

namespace TinyLogic_ok.Models.TestModels
{
    public class TestContent
    {
        public string Title { get; set; }

        public string Instructions { get; set; }

        public int TimeLimit { get; set; }

        public List<TestQuestion> Questions { get; set; }
    }
}
