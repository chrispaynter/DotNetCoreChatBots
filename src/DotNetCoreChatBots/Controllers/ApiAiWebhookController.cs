
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paynter.ApiAi.Models;
using Paynter.Harvest.Models;

namespace DotNetCoreChatBots.Controllers
{
    public class ApiAiWebhookController : Controller
    {
        private LogTimeChatBotActions _actions;
        public ApiAiWebhookController(LogTimeChatBotActions actions)
        {
            _actions = actions;
        }

        [HttpPost("api/aiapiwebhook")]
        public async Task<object> ApiAiWebhook([FromBody]ApiAiQueryResponse response)
        {
            return await _actions.SwitchActions(response);
        }
    }
}