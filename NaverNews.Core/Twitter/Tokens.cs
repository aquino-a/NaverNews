namespace NaverNews.Core
{
    public class Tokens
    {
        public string Access { get; set; }

        /// <summary>
        /// Uses UTC.
        /// </summary>
        public DateTime ExpireTime { get; internal set; }

        public string Refresh { get; set; }
        public string TokensId { get; set; }
    }
}