using System.Threading.Tasks;
using DotNetCoreChatBots.Controllers;
using Microsoft.Extensions.Logging;
using Paynter.FacebookMessenger.Models.Webhooks;
using Paynter.FacebookMessenger.Services;
using Paynter.WitAi.Exceptions;
using Paynter.WitAi.Models;
using Paynter.WitAi.Services;

namespace DotNetCoreChatBots
{
    public class ChatBotHelper
    {
        private FacebookMessengerService _facebookMessengerService;
        private WitAiService _witAiService;
        private WitSessionHelper _witSessionHelper;
        private ILogger<FacebookWebhookController> _logger;
        
        public ChatBotHelper(ILogger<FacebookWebhookController> logger, FacebookMessengerService facebookMessengerService, WitAiService witAiService, WitSessionHelper witSessionHelper)
        {
            _facebookMessengerService = facebookMessengerService;
            _witAiService = witAiService;
            _witSessionHelper = witSessionHelper;
            _logger = logger;

            _facebookMessengerService.MessageRecieved += FacebookMessageHandler;
        }

        private async Task<dynamic> Send(WitConverseRequest request, WitConverseResponse response)
        {
            var session = _witSessionHelper.FindBySessionId(request.SessionId);
            if(session == null)
            {
                // Throw an error as there's no sender to send to
            }

            // TODO: Should this be awaited? It's one way, shouldn't matter?
            _facebookMessengerService.SendTextMessage(session.FacebookSenderId, response.Message);

            // Sending messages is one way from the bot to the user, so context will not be updated
            return request.Context;
        }

        private async Task<dynamic> GetForecast(WitConverseRequest request, WitConverseResponse response)
        {
            return new { forecast = $"It's going to be sunny!" };
        }

        public async void FacebookMessageHandler(WebhookMessaging messageEvent)
        {
            var senderId = messageEvent.Sender.Id;
            var messageText = messageEvent.Message.Text;

            _facebookMessengerService.SendMarkSeen(senderId);

            // Send typing indicator if response creation takes time
            _facebookMessengerService.SendTypingOn(senderId);

            // Do something with the user's message (e.g. Call out to WitAi)
            // var witResponse = await _witAiService.Message(messageText);

            var session = _witSessionHelper.FindOrCreateSession(senderId);

            try
            {
                var actions = new WitActionDictionary();
                actions.Add(nameof(Send), Send);
                actions.Add(nameof(GetForecast), GetForecast);

                var request = new WitConverseRequest(session.WitSessionId, messageText, session.Context);

                var context = await _witAiService.RunActions(request, actions);


                // Update this user's session state
                session.Context = context;
                
                // await Converse(session, new WitConverseRequest()
                // {
                //     SessionId = session.WitSessionId,
                //     Query = messageText,
                //     Context = session.Context
                // });
            }
            catch (WitAiServiceException e)
            {
                _facebookMessengerService.SendTextMessage(senderId, "I'm new to this AI game and am having a bit of trouble replying to your message right now. I'll get back to you soon once I figure out how to reply to this :)");
            }
            

            // Reply to the user
            // _facebookMessengerService.SendTextMessage(senderId, "Hey there!");
            
            // if (!string.IsNullOrEmpty(messageText))
            // {
            //     var response = await _witAiService.Message(messageText);
            //     var entities = response.Entities as dynamic;
            //     var intent = entities.intent[0];
            //     var intentConfidence = intent.confidence;
            //     var intentValue = intent.value;
            // }
        }

        private async Task Converse(WitSession session, WitConverseRequest request)
        {
            var response = await _witAiService.Converse(request);

            switch(response.Type)
            {
                case WitConverseType.Merge:
                    break;
                case WitConverseType.Message:
                    _facebookMessengerService.SendTextMessage(session.FacebookSenderId, response.Message);
                    break;
                case WitConverseType.Action:
                    await HandleAction(session, response);
                    break;
                case WitConverseType.Stop:
                    // WitSessionHelper.EndSession(session);
                    break;
            }
        }

        private async Task HandleAction(WitSession session, WitConverseResponse response)
        {
            switch (response.Action)
            {
                case "searchForProject":

                    // Do some search process here

                    session.Context.projectName = "Village Rocs [VC20160511]";

                    await Converse(session, new WitConverseRequest()
                    {
                        SessionId = session.WitSessionId,
                        Context = session.Context
                    });
                    break;

                case "logTime":

                    // Log the actual time

                    session.Context.duration = "2.5 hours";

                    await Converse(session, new WitConverseRequest(){
                        SessionId = session.WitSessionId,
                        Context = session.Context
                    });
                    break;
            }
        }

        // private async Task SearchForProject(WitSession session, WitConverseResponse response)
        // {
            
        // }

    }
}