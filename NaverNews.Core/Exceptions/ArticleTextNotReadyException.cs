using System.Runtime.Serialization;
using System.Text.Json;

namespace NaverNews.Core
{
    [Serializable]
    internal class ArticleTextNotReadyException : Exception
    {
        private readonly Article _article;

        public ArticleTextNotReadyException(Article article)
        {
            _article = article;
        }

        public ArticleTextNotReadyException(string? message) : base(message)
        {
        }

        public ArticleTextNotReadyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ArticleTextNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => JsonSerializer.Serialize(_article);
    }
}