using AirTeamApi.Services.Models;

namespace AirTeamApi.Services.Contract
{
    public interface IAirTeamService
    {
        Task<IEnumerable<ImageDto>> SearchByKeyword(string keyword);
    }
}
