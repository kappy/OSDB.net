using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OSDBnet.Tests
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public async Task CanSearchWithSeasonAndEpisode()
        {
            using (var client = await new Osdb().Login("en", ConfigurationManager.AppSettings["TestUserAgent"]))
            {
                var results = await client.SearchSubtitlesFromQuery("en", "Arrow", 1, 1);
                Assert.IsTrue(results.Count > 0);
            }
        }

        [TestMethod]
        public async Task CanSearchOnlyText()
        {
            using (var client = await new Osdb().Login("en", ConfigurationManager.AppSettings["TestUserAgent"]))
            {
                var results = await client.SearchSubtitlesFromQuery("en", "Arrow");
                Assert.IsTrue(results.Count > 0);
            }
        }
    }
}
