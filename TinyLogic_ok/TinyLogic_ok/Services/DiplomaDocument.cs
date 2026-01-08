using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TinyLogic_ok.Models;
using TinyLogic_ok.Models.CourseModels;

public class DiplomaDocument : IDocument
{
    private readonly User _user;
    private readonly Courses _course;

    public DiplomaDocument(User user, Courses course)
    {
        _user = user;
        _course = course;
    }

    public DocumentMetadata GetMetadata() => new DocumentMetadata();

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Content().Padding(20).Decoration(dec =>
            {
                
                dec.Before().Border(6).BorderColor(Colors.Amber.Darken2);

                dec.Content().Padding(40).Column(col =>
                {
             
                    col.Item().AlignCenter().Text("DIPLOMĂ DE ABSOLVIRE")
                        .FontSize(46)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingVertical(10).AlignCenter().Element(e =>
                    {
                        e.BorderBottom(3).BorderColor(Colors.Blue.Medium);
                    });

                    col.Item().Height(20);

                    col.Item().AlignCenter().Text("Se acordă elevului:")
                        .FontSize(22);

                   
                    col.Item().AlignCenter().Text($"{_user.FirstName} {_user.LastName}")
                        .FontSize(36)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    col.Item().Height(20);

                    col.Item().AlignCenter().Text("pentru finalizarea cursului:")
                        .FontSize(22);

                    col.Item().AlignCenter().Text(_course.CourseName)
                        .FontSize(32)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    col.Item().Height(40);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Data emiterii:\n{DateTime.Now:dd MMM yyyy}")
                            .FontSize(18)
                            .AlignLeft();

                       
                    });

                    col.Item().Height(20);

               
                    col.Item().AlignCenter()
                        .Text("TinyLogic ")
                        .FontSize(18)
                        .FontColor(Colors.Grey.Darken2);
                });
            });
        });
    }
}
