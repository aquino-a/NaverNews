using System.Runtime.Serialization;

namespace NaverNews.Core
{
    [Serializable]
    internal class ArticleSummaryNotReadyException : Exception
    {
        public ArticleSummaryNotReadyException()
        {
        }

        public ArticleSummaryNotReadyException(string? message) : base(message)
        {
        }

        public ArticleSummaryNotReadyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ArticleSummaryNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}