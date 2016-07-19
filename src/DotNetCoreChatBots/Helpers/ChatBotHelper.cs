using System.Threading.Tasks;
using DotNetCoreChatBots.Controllers;
using Microsoft.Extensions.Logging;
using Paynter.FacebookMessenger.Models.Webhooks;
using Paynter.FacebookMessenger.Services;
using Paynter.WitAi.Collections;
using Paynter.WitAi.Exceptions;
using Paynter.WitAi.Models;
using Paynter.WitAi.Services;
using Paynter.WitAi.Sessions;

namespace DotNetCoreChatBots.Helpers
{
    public class ChatBotHelper
    {
        private FacebookMessengerService _facebookMessengerService;
        private WitAiService _witAiService;
        private WitSessionHelper _witSessionHelper;
        private ILogger<FacebookWebhookController> _logger;
        private bool _isListening;
        
        public ChatBotHelper(ILogger<FacebookWebhookController> logger, FacebookMessengerService facebookMessengerService, WitAiService witAiService, WitSessionHelper witSessionHelper)
        {
            _facebookMessengerService = facebookMessengerService;
            _witAiService = witAiService;
            _witSessionHelper = witSessionHelper;
            _logger = logger;
        }

        public void StartListening()
        {
            if(!_isListening)
            {
                _facebookMessengerService.MessageRecieved += FacebookMessageHandler;
                _isListening = true;
            }
        }

        public void StopListening()
        {
            _facebookMessengerService.MessageRecieved -= FacebookMessageHandler;
            _isListening = false;
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

                // Update this user's session context
                session.Context = context;
            }
            catch (WitAiServiceException e)
            {
                _logger.LogError("Recieved an exception from WitAiService, {@exception}", e);
                _facebookMessengerService.SendTextMessage(senderId, "I'm new to this AI game and am having a bit of trouble replying to your message right now. I'll get back to you soon once I figure out how to reply to this :)");
            }
        }

        private async Task<dynamic> Send(WitConverseRequest request, WitConverseResponse response)
        {
            var session = _witSessionHelper.FindBySessionId(request.SessionId);
            if(session == null)
            {
                // Throw an error as there's no sender to send to
            }

            // TODO: Should this be awaited? It's one way, shouldn't matter?
            _facebookMessengerService.SendTextMessage(session.UserId, response.Message);

            // Sending messages is one way from the bot to the user, so context will not be updated
            return request.Context;
        }

        private async Task<dynamic> GetForecast(WitConverseRequest request, WitConverseResponse response)
        {

            return new { forecast = $"sunny 32 degrees" };
        }
    }
}