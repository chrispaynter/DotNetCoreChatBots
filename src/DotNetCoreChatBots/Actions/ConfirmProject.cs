using System.Threading.Tasks;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots
{
    public partial class LogTimeChatBotActions
    {
        public async Task<object> ConfirmProject(ApiAiQueryResponse response)
        {
            var contextSummary = new LogTimeContextSummary(response);
            contextSummary.HasProject = true;

            string replyText;
            ApiAiQueryContext nextActionContext;

            var branchAction = ChooseNextStep(contextSummary, out replyText, out nextActionContext);

            var hasProjectContext = new ApiAiQueryContext("has-project");


            if(!string.IsNullOrEmpty(branchAction))
            {
                // We have a branch action, so rather than return
                // from this action, we'll run the next one.
                // First we need to update the existing response
                // for the next action.
                response.Result.Contexts.Add(hasProjectContext);
                response.Result.Action = branchAction;
                return await SwitchActions(response);
            }


            return new {
                displayText = replyText,
                speech = replyText,
                contextOut = new [] { nextActionContext, hasProjectContext } 
            };

            // This process doesn't end with this action, we'll need to run another one.

        }

        
    }
}