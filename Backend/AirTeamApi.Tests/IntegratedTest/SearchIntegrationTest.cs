using AirTeamApi.Services.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirTeamApi.Tests.IntegratedTest
{
    [TestClass]
    public class SearchIntegrationTest : BaseIntegrationTest
    {
        [TestMethod]
        public async Task SearchByClient()
        {
            await StartServer();
            var client = await GetClient();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            if (baseDir == null)
                throw new NullReferenceException("baseDirectory of test app is null");

            var sampleFilePath = Path.Combine(baseDir, "sampleResponse.txt");
            string resultHtml = File.ReadAllText(sampleFilePath);

            DistributedCache.Setup(ca => ca.GetAsync("777x", It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((key, token) =>
                {
                    var bytes = Encoding.UTF8.GetBytes(resultHtml);
                    return Task.FromResult(bytes);
                });

            var response = await client.GetAsync("/v1/AirTeam/Search?keyword=777x");
            var jsonResult = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(response.IsSuccessStatusCode);
            var jArray = JArray.Parse(jsonResult);

            Assert.AreEqual(25, jArray.Count);

            var firstItem = jArray[0].ToObject<ImageDto>();

            Assert.AreEqual("353153", firstItem.ImageId);
            Assert.AreEqual("Boeing 777-9X", firstItem.Title);
            StringAssert.EndsWith(firstItem.BaseImageUrl, "pics/353/353153_200.jpg");

        }
    }
}
