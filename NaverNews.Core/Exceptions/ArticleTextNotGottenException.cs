using System.Runtime.Serialization;

namespace NaverNews.Core
{
    [Serializable]
    internal class ArticleTextNotGottenException : Exception
    {
        public ArticleTextNotGottenException()
        {
        }

        public ArticleTextNotGottenException(string? message) : base(message)
        {
        }

        public ArticleTextNotGottenException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ArticleTextNotGottenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}