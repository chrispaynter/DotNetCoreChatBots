using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots
{
    public partial class LogTimeChatBotActions
    {
        public async Task<object> ConfirmTaskIndex(ApiAiQueryResponse response)
        {
            var taskIndex = (int)response.Result.Parameters["taskIndex"];
            var taskIdString = (string)response.Result.Parameters["taskIds"];

            var taskIds = taskIdString.Split('|');
            var taskId = taskIds[taskIndex];

            var hasTaskContext = new ApiAiQueryContext("has-task", parameters:JObject.FromObject(new {
                taskId = taskId
            }));


            var contextSummary = new LogTimeContextSummary(response);
            contextSummary.HasTask = true;

            var replyText = "";
            ApiAiQueryContext nextActionContext;

            var branchAction = ChooseNextStep(contextSummary, out replyText, out nextActionContext);

            if(!string.IsNullOrEmpty(branchAction))
            {
                // We have a branch action, so rather than return
                // from this action, we'll run the next one.
                // First we need to update the existing response
                // for the next action.
                response.Result.Contexts.Add(hasTaskContext);
                response.Result.Action = branchAction;
                return await SwitchActions(response);
            }


            return new {
                displayText = replyText,
                speech = replyText,
                contextOut = new [] { hasTaskContext, nextActionContext } 
            };
        }
    }
}