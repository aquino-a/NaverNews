using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverNews.Core
{
    public class NaverClient
    {
        private const string BASE_URL = "https://news.naver.com/";
        private const string SECTION_PATH = "main/main.naver";

        private readonly HttpClient _httpClient;

        public NaverClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets basic article data from naver.
        /// </summary>
        /// <param name="type">the type of news section to get.</param>
        /// <param name="pages">amount of pages deep to check</param>
        /// <returns></returns>
        public Article GetArticles(NewsType type, int pages)
        {
            //mode=LSD&mid=shm&sid1=102
            //first list //sid1=102&firstLoad=Y  
            // comment counts //resultType=MAP&ticket=news&lang=ko&country=KR&objectId=news001,0014121080&objectId=news001,0014122715&objectId=news055,0001080206&objectId=news022,0003843225&objectId=news009,0005170711&objectId=news057,0001761430&objectId=news421,0006982201&objectId=news005,0001629825&objectId=news003,0012022543&objectId=news023,0003780680&objectId=news214,0001291803&objectId=news214,0001291802&objectId=news214,0001291537&objectId=news277,0005298389&objectId=news014,0005055168&objectId=news277,0005298385&objectId=news008,0004923507&objectId=news055,0001080338&objectId=news421,0006982405&objectId=news016,0002181672&objectId=news422,0000613595&objectId=news052,0001921379&objectId=news016,0002181670&objectId=news006,0000119269&objectId=news009,0005170770&objectId=news056,0011543018&objectId=news011,0004224778&objectId=news586,0000062604&objectId=news028,0002651718&objectId=news003,0012023609&objectId=news025,0003299815&objectId=news469,0000754369&objectId=news018,0005548118

            // next pages //sid1=102&date=%2000:00:00&page=2
            //comments - same as above
        }
    }
}
