
using Microsoft.EntityFrameworkCore;
using TinyLogic_ok.Models;
using TinyLogic_ok.Services;
using TinyLogic_ok.Models.LessonModels;

public class LessonProgressService : ILessonProgressService
{
    private readonly TinyLogicDbContext _context;

    public LessonProgressService(TinyLogicDbContext context)
    {
        _context = context;
    }

    public async Task MarkLessonCompletedAsync(int userId, int lessonId)
    {
        var existing = await _context.UserLessons
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId);

        if (existing != null)
            return;

        var progress = new UserLessons
        {
            UserId = userId,
            LessonId = lessonId,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        _context.UserLessons.Add(progress);
        await _context.SaveChangesAsync();
    }
}
