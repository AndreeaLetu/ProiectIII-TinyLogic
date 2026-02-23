using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
        public string MatchedQuestion { get; set; }
    }

    public interface IRagService
    {
        Task<RagResult> SearchAsync(string query);
    }

    public class RagService : IRagService
    {
        private readonly IWebHostEnvironment _env;
        private List<KnowledgeBaseItem> _cachedKnowledge;
        private DateTime _lastLoadTime;

        public RagService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<RagResult> SearchAsync(string query)
        {
            // Load knowledge base (with caching)
            var items = await LoadKnowledgeBaseAsync();

            if (items == null || !items.Any())
            {
                Console.WriteLine("⚠️ Knowledge base is empty!");
                return new RagResult
                {
                    BestMatchAnswer = "",
                    Similarity = 0,
                    MatchedQuestion = ""
                };
            }

            // Find best match
            var matches = items
                .Select(i => new {
                    Item = i,
                    Score = ComputeSimilarity(query, i.Question)
                })
                .Where(m => m.Score > 0)
                .OrderByDescending(m => m.Score)
                .ToList();

            if (!matches.Any())
            {
                Console.WriteLine($"❌ No matches found for: '{query}'");
                return new RagResult
                {
                    BestMatchAnswer = "",
                    Similarity = 0,
                    MatchedQuestion = ""
                };
            }

            var bestMatch = matches.First();
            Console.WriteLine($"✅ Best match: '{bestMatch.Item.Question}' (Score: {bestMatch.Score:F2})");

            return new RagResult
            {
                BestMatchAnswer = bestMatch.Item.Answer,
                Similarity = bestMatch.Score,
                MatchedQuestion = bestMatch.Item.Question
            };
        }

        private async Task<List<KnowledgeBaseItem>> LoadKnowledgeBaseAsync()
        {
            // Cache for 5 minutes
            if (_cachedKnowledge != null &&
                (DateTime.Now - _lastLoadTime).TotalMinutes < 5)
            {
                return _cachedKnowledge;
            }

            string file = Path.Combine(_env.WebRootPath, "data", "knowledge.json");

            if (!File.Exists(file))
            {
                Console.WriteLine($"⚠️ Knowledge file not found: {file}");
                return new List<KnowledgeBaseItem>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(file);
                _cachedKnowledge = JsonConvert.DeserializeObject<List<KnowledgeBaseItem>>(json);
                _lastLoadTime = DateTime.Now;

                Console.WriteLine($"✅ Loaded {_cachedKnowledge?.Count ?? 0} knowledge items");
                return _cachedKnowledge ?? new List<KnowledgeBaseItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading knowledge base: {ex.Message}");
                return new List<KnowledgeBaseItem>();
            }
        }

        private float ComputeSimilarity(string query, string question)
        {
            // Normalize both strings
            string normalizedQuery = Normalize(query);
            string normalizedQuestion = Normalize(question);

            if (string.IsNullOrWhiteSpace(normalizedQuery) ||
                string.IsNullOrWhiteSpace(normalizedQuestion))
                return 0f;

            // 1. Check for exact substring match
            if (normalizedQuestion.Contains(normalizedQuery))
                return 1.0f;

            if (normalizedQuery.Contains(normalizedQuestion))
                return 0.9f;

            // Extract keywords
            var queryWords = GetKeywords(normalizedQuery);
            var questionWords = GetKeywords(normalizedQuestion);

            if (!queryWords.Any() || !questionWords.Any())
                return 0f;

            // 2. Calculate word overlap (Jaccard similarity)
            var intersection = queryWords.Intersect(questionWords).Count();
            var union = queryWords.Union(questionWords).Count();
            float jaccardScore = (float)intersection / union;

            // 3. Calculate keyword coverage (how many query words are in question)
            float keywordCoverage = (float)intersection / queryWords.Count();

            // 4. Bonus for word order similarity
            float orderBonus = 0f;
            var queryList = queryWords.ToList();
            var questionList = questionWords.ToList();

            for (int i = 0; i < Math.Min(queryList.Count, questionList.Count); i++)
            {
                if (i < queryList.Count && i < questionList.Count &&
                    queryList[i] == questionList[i])
                {
                    orderBonus += 0.1f;
                }
            }
            orderBonus = Math.Min(orderBonus, 0.2f);

            // 5. Weighted combination
            float finalScore =
                (jaccardScore * 0.4f) +      // Word overlap
                (keywordCoverage * 0.4f) +   // Query coverage
                orderBonus;                   // Word order

            return Math.Min(finalScore, 1f);
        }

        private string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = text.ToLower();

            // Remove Romanian diacritics
            text = text
                .Replace("ă", "a").Replace("â", "a").Replace("î", "i")
                .Replace("ș", "s").Replace("ş", "s")
                .Replace("ț", "t").Replace("ţ", "t");

            // Remove punctuation but keep spaces
            text = Regex.Replace(text, @"[^\w\s]", " ");

            // Normalize whitespace
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        private HashSet<string> GetKeywords(string text)
        {
            // Romanian stop words (common words to ignore)
            var stopWords = new HashSet<string>
            {
                "ce", "este", "un", "o", "de", "la", "in", "si", "cu", "pe", "pentru",
                "din", "sau", "mai", "ca", "cum", "care", "sunt", "este", "a", "ai",
                "am", "au", "as", "ar", "sa", "se", "va", "vor", "fi", "fost", "fie",
                "fara", "sub", "dupa", "el", "ea", "ei", "ele", "despre", "asta",
                "acesta", "aceasta", "acest", "aceste", "cand", "unde", "care", "ce",
                "tot", "toate", "doar", "nici", "nimic", "nimeni", "poate", "putem"
            };

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .ToHashSet();

            return words;
        }
    }
}