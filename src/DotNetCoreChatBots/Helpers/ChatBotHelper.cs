using System.Linq;
using System.Threading.Tasks;
using DotNetCoreChatBots.Controllers;
using Microsoft.Extensions.Logging;
using Paynter.FacebookMessenger.Models.Webhooks;
using Paynter.FacebookMessenger.Services;
using Paynter.WitAi.Actions;
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
        private HarvestDataHelper _harvestDataHelper;
        private bool _isListening;
        
        public ChatBotHelper(ILogger<FacebookWebhookController> logger, FacebookMessengerService facebookMessengerService, WitAiService witAiService, WitSessionHelper witSessionHelper, HarvestDataHelper harvestDataHelper)
        {
            _facebookMessengerService = facebookMessengerService;
            _witAiService = witAiService;
            _witSessionHelper = witSessionHelper;
            _logger = logger;
            _harvestDataHelper = harvestDataHelper;

            // This is done here but ultimately will be done on a scheudle
            // probably once or twice a day.
            _harvestDataHelper.RefreshProjectsList();
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
                actions.Add(nameof(FindProject), FindProject);
                actions.Add(nameof(ClearSession), ClearSession);

                var request = new WitConverseRequest(session.WitSessionId, messageText, session.Context);
                // var actions = new TestWitActions(_witSessionHelper, _facebookMessengerService);


                var context = await _witAiService.RunActions(request, actions);

                // Need to work out how to indicate an ended session
                //_witSessionHelper.EndSession(request.SessionId);
                
                session.Context = context;
            }
            catch (WitAiServiceException e)
            {
                _logger.LogError("Recieved an exception from WitAiService, {@exception}", e);
                _facebookMessengerService.SendTextMessage(senderId, "I'm new to this AI game and am having a bit of trouble replying to your message right now. I'll get back to you soon once I figure out how to reply to this :)");
            }
        }

        private async Task<dynamic> ClearSession(WitConverseRequest request, WitConverseResponse response)
        {
            return null;
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

        private async Task<dynamic> FindProject(WitConverseRequest request, WitConverseResponse response)
        {
            var project = response.GetFirstEntityValue("project");
            var possibleProjects = _harvestDataHelper.QueryProjectsByName(project);
            var count = possibleProjects.Count();

            if(count > 1)
            {
                return new { projects = possibleProjects.Select(u => string.Join(u.Name, ",")) };
            }
            else if(count == 1)
            {
                return new { project = possibleProjects.FirstOrDefault().Name };
            } 
            else
            {
                return new { notFound = true };
            }
        } 

               
    }
}