using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Impl;
using AirTeamApi.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirTeamApi.Tests.UnitTest
{
    [TestClass]
    public class AirTeamCtrlTest
    {
        [TestMethod]
        public async Task Search()
        {
            var MockedClient = new Moq.Mock<IAirTeamHttpClient>();
            var htmlParseService = new HtmlParseService();
            var MockedCache = new Moq.Mock<IDistributedCache>();
            var MockedIOptions = new Moq.Mock<IOptions<AirTeamSetting>>();
            MockedIOptions.SetupGet(o => o.Value).Returns(new AirTeamSetting { CacheExprationMinutes = 15 });

            var actualService = new AirTeamService(MockedCache.Object, MockedClient.Object, htmlParseService, MockedIOptions.Object);

            var airTeamController = new Controllers.AirTeamController(actualService);


            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var sampleFilePath = Path.Combine(baseDir, "sampleResponse.txt");
            string resultHtml = File.ReadAllText(sampleFilePath);
            var keyword = "777x";

            #region get from httpClient
            var calledHttp = false;
            MockedClient.Setup(cl => cl.SearchByKeyword(Moq.It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    calledHttp = true;
                    return Task.FromResult(resultHtml);                    
                });

            MockedCache.Setup(ca => ca.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((key, token) =>
                {
                    return Task.FromResult<byte[]>(null);
                });


            var resultImages = await airTeamController.Search(keyword);

            Assert.AreEqual(25, resultImages.Count());
            Assert.IsTrue(calledHttp);
            #endregion
           
            #region get from cache
            MockedClient.Setup(cl => cl.SearchByKeyword(Moq.It.IsAny<string>())).Throws(new InvalidOperationException("Must Use Cache"));

            var calledCache = false;
            MockedCache.Setup(ca => ca.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((key, token) =>
                {
                    calledCache = true;
                    var bytes = Encoding.UTF8.GetBytes(resultHtml);
                    return Task.FromResult(bytes);
                });

            resultImages = await airTeamController.Search(keyword);
            
            Assert.AreEqual(25, resultImages.Count());
            Assert.IsTrue(calledCache);
            #endregion


            #region check result fields
            
            var firstItem = resultImages.First();
            Assert.AreEqual("353153", firstItem.ImageId);
            Assert.AreEqual("Boeing 777-9X", firstItem.Title);
            Assert.IsFalse(string.IsNullOrWhiteSpace(firstItem.DetailUrl));
            StringAssert.EndsWith(firstItem.BaseImageUrl, "pics/353/353153_200.jpg");

            #endregion
        }
    }
}
