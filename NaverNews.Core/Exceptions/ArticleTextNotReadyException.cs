using System.Runtime.Serialization;

namespace NaverNews.Core
{
    [Serializable]
    internal class ArticleTextNotReadyException : Exception
    {
        public ArticleTextNotReadyException()
        {
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
    }
}