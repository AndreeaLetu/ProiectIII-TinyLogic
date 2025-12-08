namespace TinyLogic_ok.Services
{
    public interface IAiService
    {
        Task<string> GenerateAsync(string prompt);
    }
}
