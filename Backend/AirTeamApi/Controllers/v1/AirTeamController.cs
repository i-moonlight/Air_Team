using AirTeamApi.Services.Contract;
using AirTeamApi.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AirTeamApi.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AirTeamController : ControllerBase
    {
        private readonly IAirTeamService AirTeamService;
        private readonly ILogger<AirTeamController> Logger;
        public AirTeamController(IAirTeamService airTeamService, ILogger<AirTeamController> logging)
        {
            AirTeamService = airTeamService;
            Logger = logging;
        }

        /// <summary>
        /// test serilog seq logging in production
        /// </summary>
        /// <param name="Message">simple text to log as error message</param>
        /// <returns></returns>
        [HttpGet("testlog")]
        public void TestLog([Required(AllowEmptyStrings = false), MaxLength(100)] string message)
        {
            Logger.LogWarning(message);
        }

        /// <summary>
        /// Search AirTeamImages with specified keyword
        /// </summary>
        /// <param name="keyword">keyword to search</param>
        /// <remarks>
        /// /AirTeam/Search?keyword=iriaf
        /// </remarks>
        /// <returns>json array of search results</returns>
        /// <response code="200">if every thing goes well</response>
        /// <response code="400">If keyword not supplied and has length less than 3</response>     
        [HttpGet("Search")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<ImageDto>> Search([MinLength(3), MaxLength(14), Required(AllowEmptyStrings = false), FromQuery] string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException(keyword);

            if (keyword.Trim().Length < 3)
                return new List<ImageDto>();

            var result = await AirTeamService.SearchByKeyword(keyword);
            return result;
        }
    }
}
