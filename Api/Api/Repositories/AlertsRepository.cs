using Api.Data;
using Api.Dto;
using Microsoft.EntityFrameworkCore;


namespace Api.Repositories
{
    public class AlertsRepository: IAlertsRepository
    {
        private AppDbContext _context;

        public AlertsRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CommandCountDto> GetAllAsync()
        {
            var counts = await Task.WhenAll(
                _context.NorthAlerts.CountAsync(),
                _context.CenterAlerts.CountAsync(),
                _context.SouthAlerts.CountAsync(),
                _context.OverseasAlerts.CountAsync()
            );

            return new CommandCountDto
            {
                NothAlert = counts[0],
                CentralAlert = counts[1],
                SouthAlert = counts[2],
                OverseasAlert = counts[3]
            };
        }
    }
}
