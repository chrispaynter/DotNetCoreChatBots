using Microsoft.AspNetCore.Mvc;
using Paynter.WitAi;
using Paynter.FacebookMessenger.Models.Webhooks;
using Paynter.FacebookMessenger.Services;
using Microsoft.Extensions.Logging;
using Paynter.WitAi.Services;

namespace DotNetCoreChatBots.Controllers
{
    public class FacebookWebhookController : Controller
    {
        private FacebookMessengerService _facebookMessengerService;
        private WitAiService _witAiService;

        public FacebookWebhookController(ILogger<FacebookWebhookController> logger, FacebookMessengerService facebookMessengerService, WitAiService witAiService)
        {
            _facebookMessengerService = facebookMessengerService;
            _witAiService = witAiService;

            _facebookMessengerService.MessageRecieved += MessageRecieved;
        }

        [HttpGet("api/facebookwebhook")]
        public object Index(WebhookHubRequest hub)
        {
            if (_facebookMessengerService.ValidateHubRequest(hub))
            {
                return hub.Challenge;
            }
            return NotFound();
        }

        [HttpPost("api/facebookwebhook")]
        public void Index([FromBody]WebhookCallback request)
        {
            _facebookMessengerService.ProcessWebhookRequest(request);
        }

        private async void MessageRecieved(WebhookMessaging messageEvent)
        {
            var senderId = messageEvent.Sender.Id;
            var messageText = messageEvent.Message.Text;

            _facebookMessengerService.SendMarkSeen(senderId);

            // Send typing indicator if response creation takes time
            _facebookMessengerService.SendTypingOn(senderId);

            // Do something with the user's message (e.g. Call out to WitAi)
            var witResponse = await _witAiService.Message(messageText);

            // Reply to the user
            _facebookMessengerService.SendTextMessage(senderId, "Hey there!");
            
            // if (!string.IsNullOrEmpty(messageText))
            // {
            //     var response = await _witAiService.Message(messageText);
            //     var entities = response.Entities as dynamic;
            //     var intent = entities.intent[0];
            //     var intentConfidence = intent.confidence;
            //     var intentValue = intent.value;
            // }
        }



        





        // private async Task SendGenericMessage(string recipientId)
        // {
        //     var messageData = new
        //     {
        //         recipient = new
        //         {
        //             id = recipientId
        //         },
        //         message = new
        //         {
        //             attachment = new
        //             {
        //                 type = "template",
        //                 payload = new
        //                 {
        //                     template_type = "generic",
        //                     elements = new[]{
        //                     new {
        //                         title = "rift",
        //                         subtitle = "Next-generation virtual reality",
        //                         item_url = "https://www.oculus.com/en-us/rift/",
        //                         image_url = "http://messengerdemo.parseapp.com/img/rift.png",
        //                         buttons = new[]{
        //                             new {
        //                                 type = "web_url",
        //                                 url = "https://www.oculus.com/en-us/rift/",
        //                                 title = "Open Web URL"
        //                             }

        //                         }
        //                     }
        //                 }
        //                 }
        //             }
        //         }
        //     };

        //     await _facebookMessengerService.CallSendApi(messageData);
        // }
    }
}
