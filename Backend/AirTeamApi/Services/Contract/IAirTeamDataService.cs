using AirTeamApi.Services.Models;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamDataService
    {
        Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword);
    }
}
