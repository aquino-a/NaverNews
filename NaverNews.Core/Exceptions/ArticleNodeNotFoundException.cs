using System.Runtime.Serialization;

namespace NaverNews.Core
{
    [Serializable]
    internal class ArticleNodeNotFoundException : Exception
    {
        public ArticleNodeNotFoundException()
        {
        }

        public ArticleNodeNotFoundException(string? message) : base(message)
        {
        }

        public ArticleNodeNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ArticleNodeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}