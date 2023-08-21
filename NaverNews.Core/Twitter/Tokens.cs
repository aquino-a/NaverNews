namespace NaverNews.Core
{
    public class Tokens
    {
        public string Refresh { get; set; }
        public string Access { get; set; }

        /// <summary>
        /// Uses UTC.
        /// </summary>
        public DateTime ExpireTime { get; internal set; }
    }
}