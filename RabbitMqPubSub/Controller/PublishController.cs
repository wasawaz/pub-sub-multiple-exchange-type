using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMqPubSub.Commons.Dispatchers;
using RabbitMqPubSub.Messages.Commands;

namespace RabbitMqPubSub.Controller
{
    [Route("api/[controller]")]
    public class PublishController : ControllerBase
    {
        private IDispatcher _dispatcher;

        public PublishController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpPost]
        [Route("PublishUserFanOut")]
        public async Task<IActionResult> PublishUserFanOut(UserCreate command)
        {
            await _dispatcher.SendAsync(command);
            return Accepted();
        }

        [HttpPost]
        [Route("PublishProjectDirect")]
        public async Task<IActionResult> PublishProjectDirect(ProjectCreate command)
        {
            await _dispatcher.SendAsync(command);
            return Accepted();
        }
    }
}