using Api.Dto;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly IAlertsRepository _repository;
        public AlertController(IAlertsRepository repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<CommandCountDto> GetAllAsync()
                => await _repository.GetAllAsync();

    }
}
