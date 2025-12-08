using Newtonsoft.Json;

namespace TinyLogic_ok.Services
{
    public class KnowledgeBaseItem
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class RagResult
    {
        public string BestMatchAnswer { get; set; }
        public float Similarity { get; set; }
    }

    public interface IRagService
    {
        Task<RagResult> SearchAsync(string query);
    }

    public class RagService : IRagService
    {
        private readonly IWebHostEnvironment _env;

        public RagService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<RagResult> SearchAsync(string query)
        {
            string file = Path.Combine(_env.WebRootPath, "data", "knowledge.json");

            if (!File.Exists(file))
            {
                return new RagResult { BestMatchAnswer = "", Similarity = 0 };
            }

            var json = await File.ReadAllTextAsync(file);
            var items = JsonConvert.DeserializeObject<List<KnowledgeBaseItem>>(json);

            var match = items
                .Select(i => new {
                    Item = i,
                    Score = ComputeSimilarity(query, i.Question)
                })
                .OrderByDescending(m => m.Score)
                .FirstOrDefault();

            return new RagResult
            {
                BestMatchAnswer = match?.Item.Answer ?? "",
                Similarity = match?.Score ?? 0
            };
        }

        private float ComputeSimilarity(string a, string b)
        {
            a = a.ToLower();
            b = b.ToLower();

            if (b.Contains(a)) return 1f;
            return 0f;
        }
    }
}
