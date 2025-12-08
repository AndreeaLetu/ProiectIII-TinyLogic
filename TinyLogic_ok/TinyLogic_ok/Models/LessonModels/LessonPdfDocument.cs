using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TinyLogic_ok.Models.LessonModels;

public class LessonPdfDocument : IDocument
{
    private readonly string _courseName;
    private readonly List<(Lessons Lesson, LessonContent Content)> _allLessons;

    public LessonPdfDocument(string courseName, List<(Lessons, LessonContent)> lessons)
    {
        _courseName = courseName;
        _allLessons = lessons;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata
    {
        Title = _courseName
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            page.Header()
                .Text(_courseName)
                .FontSize(32)
                .Bold()
                .AlignCenter()
                .FontColor("#6C5CE7");

            page.Content().Column(col =>
            {
                foreach (var item in _allLessons)
                {
                    var lesson = item.Lesson;
                    var content = item.Content;

                    col.Item().PaddingTop(20).Text(lesson.LessonName)
                        .FontSize(24).Bold().FontColor("#4834D4");

                    if (content.Sections != null)
                    {
                        foreach (var s in content.Sections)
                        {
                            col.Item().PaddingTop(10).Text(s.Heading)
                                .FontSize(18).Bold();

                            col.Item().Text(s.Text)
                                .FontSize(12)
                                .LineHeight(1.4f);
                        }
                    }

                    if (content.Exercise != null)
                    {
                        col.Item().PaddingTop(15).Text("Exercițiu")
                            .FontSize(20).Bold().FontColor("#00A8FF");

                        col.Item().Text(content.Exercise.Description)
                            .FontSize(12);

                        col.Item().Text($"Output așteptat:")
                            .FontSize(14).Bold();

                        col.Item().PaddingBottom(8)
                            .Text(content.Exercise.ExpectedOutput)
                            .FontSize(12).Italic();
                    }

                    col.Item().PageBreak(); // separă lecțiile pe pagini
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span($"Generat automat cu TinyLogic • {DateTime.Now:dd.MM.yyyy}")
                    .FontSize(10).FontColor("#888");
            });
        });
    }
}
