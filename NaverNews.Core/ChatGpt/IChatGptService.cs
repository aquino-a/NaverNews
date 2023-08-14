namespace NaverNews.Core
{
    public interface IChatGptService
    {
        Task<string> Summarize(string text);
        Task<string> Translate(string text);
    }
}