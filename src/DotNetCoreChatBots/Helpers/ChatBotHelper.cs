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
        private ILogger<FacebookWebhookController> _logger;
        public ChatBotHelper(ILogger<FacebookWebhookController> logger, FacebookMessengerService facebookMessengerService, WitAiService witAiService)
        {
            _facebookMessengerService = facebookMessengerService;
            _witAiService = witAiService;
            _logger = logger;

            _facebookMessengerService.MessageRecieved += FacebookMessageHandler;
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

            var session = WitSessionHelper.GetSession(senderId);

            if(session == null)
            {
                session = WitSessionHelper.CreateSession(senderId);
                _logger.LogDebug("Created new Wit session: {senderId} {sessionId}", session.FacebookSenderId, session.WitSessionId);
            }

            try
            {
                await Converse(session, new WitConverseRequest()
                {
                    SessionId = session.WitSessionId,
                    Query = messageText,
                    Context = session.Context
                });
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