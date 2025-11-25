namespace TinyLogic_ok.Services
{
    public interface ILessonProgressService
    {
        Task MarkLessonCompletedAsync(int userId, int lessonId);

    }
}
