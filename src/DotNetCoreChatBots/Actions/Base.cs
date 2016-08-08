using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots
{
    public partial class LogTimeChatBotActions
    {
        private HarvestDataHelper _harvestDataHelper;
        private LogTimeChatBotOptions _options;

        public LogTimeChatBotActions(HarvestDataHelper harvestDataHelper, IOptions<LogTimeChatBotOptions> options)
        {
            _harvestDataHelper = harvestDataHelper;
            _options = options.Value;
        }

        public LogTimeChatBotUser GetUser(ApiAiQueryResponse response)
        {
            // We extract the client details from the context that was passed from API.AI
            // This gets set as a context object by the client. So for example, there
            // is a Facebook Messenger webhook running as a node app in Heroku which is
            // parsing the user's request from Facebook, then setting a context called
            // "client-user" that has the paremeters "client-user.client" and "client-user.user"
            var clientUserContext = response.Result.Contexts.FirstOrDefault(u => u.Name == "client-user");
            if(clientUserContext == null)
            {
                return null;
            }

            var clientUserClient = (string) clientUserContext.Parameters["client-user.client"];
            var clientUserUser = (string) clientUserContext.Parameters["client-user.user"];

            if(string.IsNullOrEmpty(clientUserClient) || string.IsNullOrEmpty(clientUserUser))
            {
                return null;
            }


            // Next try and find the appropriate authorised user out of the LogTimeChatBot options.
            LogTimeChatBotUser user = null;

            switch (clientUserClient)
            {
                case "facebook":
                    user = _options.Users.FirstOrDefault(u => u.FacebookId == clientUserUser);
                    break;
                default:
                    user = null;
                    break;
            }

            return user;
        }

        public object UnauthorisedMessage()
        {
            var message = "Sorry it looks like you haven't been provisioned to use me yet :/ Ask Chris to get you up and running.";
            return new {
                displayText = message,
                speech = message,
                contextOut = new [] { new ApiAiQueryContext("not-provisioned", 1) } 
            };
        }

        public async Task<object> SwitchActions(ApiAiQueryResponse response)
        {
            switch (response.Result.Action)
            {
                case "LogTime.FindProject":
                    return await FindProject(response);
                case "LogTime.FindTasks":
                    return await FindTasks(response);
                case "LogTime.ConfirmProject":
                    return await ConfirmProject(response);
                case "LogTime.LogTime":
                    return await LogTime(response);
                case "LogTime.ConfirmTaskIndex":
                    return await ConfirmTaskIndex(response);
            }

            return null;
        }

        public string ChooseNextStep(LogTimeContextSummary contextSummary, out string replyText, out ApiAiQueryContext context)
        {
            replyText = "";
            context = null;

            // These are in a preferred order of asking for information from the user
            // in order to complete a time sheet entry
           
            if(!contextSummary.HasProject)
            {
                replyText = "Super! Now, what day do you want to log this time for?";
                context = new ApiAiQueryContext("ask-for-project");
            }

            if(!contextSummary.HasTask)
            {
                return "LogTime.FindTasks";
            }

            if(!contextSummary.HasDate)
            {
                replyText = "Super! Now, what day do you want to log this time for?";
                context = new ApiAiQueryContext("ask-for-date");
            }

            if(!contextSummary.HasTime)
            {
                replyText = "Great! Now, how long did you spend on it?";
                context = new ApiAiQueryContext("ask-for-time");
            }

            return null;
        }
    }

    public class LogTimeContextSummary
    {
        ApiAiQueryResponse _response;
        public LogTimeContextSummary(ApiAiQueryResponse response)
        {
            _response = response;
        }

        private bool? _hasTime;
        private bool? _hasProject;
        private bool? _hasDate;
        private bool? _hasDescription;
        private bool? _hasTask;

        public bool HasTime 
        { 
            get 
            {
                if(!_hasTime.HasValue)
                {
                    _hasTime = _response.Result.Contexts.FirstOrDefault(u => u.Name == "has-time") != null;
                }

                return _hasTime.Value;
            }
            set 
            {
                _hasTime = value;
            }
        }
        public bool HasProject 
        { 
            get 
            {
                if(!_hasProject.HasValue)
                {
                    _hasProject = _response.Result.Contexts.FirstOrDefault(u => u.Name == "has-project") != null;
                }

                return _hasProject.Value;
            }
            set 
            {
                _hasProject = value;
            }
        }
        public bool HasDate 
        { 
            get 
            {
                if(!_hasDate.HasValue)
                {
                    _hasDate = _response.Result.Contexts.FirstOrDefault(u => u.Name == "has-date") != null;
                }

                return _hasDate.Value;
            }
            set 
            {
                _hasDate = value;
            }
        }

        public bool HasDescription 
        { 
            get 
            {
                if(!_hasDescription.HasValue)
                {
                    _hasDescription = _response.Result.Contexts.FirstOrDefault(u => u.Name == "has-description") != null;
                }

                return _hasDescription.Value;
            }
            set 
            {
                _hasDescription = value;
            }
        }

        public bool HasTask 
        { 
            get 
            {
                if(!_hasTask.HasValue)
                {
                    _hasTask = _response.Result.Contexts.FirstOrDefault(u => u.Name == "has-task") != null;
                }

                return _hasTask.Value;
            }
            set 
            {
                _hasTask = value;
            }
        }
    }
}