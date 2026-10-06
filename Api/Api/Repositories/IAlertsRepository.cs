using Api.Dto;

namespace Api.Repositories
{
    public interface IAlertsRepository
    {
        Task<CommandCountDto> GetAllAsync();
    }
}
