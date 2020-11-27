using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Slack;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Birthday_Bot.Controllers
{
    [Route("api/slack")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public SlackController(SlackAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /*
        To send a POST request:
        Headers:
            X-Slack-Signature: Signing Secret from the Slack App
            X-Slack-Request-Timestamp: generate a timestamp. To generate enter into https://www.unixtimestamp.com/index.php
        Body: 
            select raw and send as a text the slack app token
         */
        [HttpPost]
        //[HttpGet]
        public async Task PostAsync()
        {
            try
            {
                await _adapter.ProcessAsync(Request, Response, _bot);
            }
            catch(System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
    }
}