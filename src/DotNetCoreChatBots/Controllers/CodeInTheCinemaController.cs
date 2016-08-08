
using System.Threading.Tasks;
using DotNetCoreChatBots.Helpers;
using Microsoft.AspNetCore.Mvc;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots.Controllers
{
    public class CodeInTheCinemaController : Controller
    {
        private LifxHelper _lifxHelper;

        public CodeInTheCinemaController(LifxHelper lifxHelper)
        {
            _lifxHelper = lifxHelper;
        }

        [HttpPost("api/webhook")]
        public async Task<object> ApiAiWebhook([FromBody]ApiAiQueryResponse response)
        {
            var color = GetColor(response);
            var light = "CodeInTheCinema";

            var responseText = "";


            switch(response.Result.Action)
            {
                case "TurnLightOn":

                    var isOn = await _lifxHelper.LightIsOn(light);

                    if(isOn)
                    {
                        responseText = "The light is already on :)";
                    }
                    else
                    {
                        _lifxHelper.TurnLightOn(light);
                    }

                    break;

                case "TurnLightOff":

                    var isOff = await _lifxHelper.LightIsOff(light);

                    if(isOff)
                    {
                        responseText = "You've turned it off already! :)";
                    }
                    else
                    {
                        _lifxHelper.TurnLightOff(light);
                    }

                    break;

                case "ChangeLightColor":

                    _lifxHelper.ChangeLightColor(light, color);

                    break;
            }


            if(responseText == "")
            {
                // Let Api.Ai handle responding to the user.
                return null;
            }
            else
            {
                // Send a specific response back to the user.
                return new {
                    displayText = responseText,
                    speech = responseText
                };
            }

            
        }






        // Gets the "color" parameter from the Api.Ai response
        public string GetColor(ApiAiQueryResponse response)
        {
            return (string)response.Result.Parameters["color"];
        }
    }
}