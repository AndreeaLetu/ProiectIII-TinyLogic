using Newtonsoft.Json;
using TinyLogic_ok.Models.LessonModels;

public static class LessonJsonParser
{
    public static LessonContent Parse(string json)
    {
        return JsonConvert.DeserializeObject<LessonContent>(json);
    }
}
