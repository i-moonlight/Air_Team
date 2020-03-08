using AirTeamApi.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamService
    {
        Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword);
    }
}
