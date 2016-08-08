using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots
{
    public partial class LogTimeChatBotActions
    {
        public async Task<object> FindTasks(ApiAiQueryResponse response)
        {
            var projectId = (string)response.Result.Parameters["projectId"];
            var project = _harvestDataHelper.GetProjectById(projectId);
            await _harvestDataHelper.LazyLoadTasks(project);

            var replyText = "";
            ApiAiQueryContext context = null;

            if(project.Tasks.Count() == 0)
            {
                replyText = "Sorry about this but there are no tasks to log time against for this project. You'll need to talk to the project manager in order to log time against this project.";
                context = new ApiAiQueryContext("tasks-not-found");
            }
            else 
            {
                replyText = $"Alright, I found {project.Tasks.Count()} tasks for this project. ";

                var taskIdString = "";

                var i = 1;
                foreach (var task in project.Tasks.Select(u => u.Task))
                {
                    taskIdString += task.Id + "|";
                    replyText += $"({i}) {task.Name}, ";
                    i++;
                }
                replyText.TrimEnd(new[] { ' ', ',' });
                taskIdString.TrimEnd('|');

                context = new ApiAiQueryContext("tasks-ask-multiple", parameters:JObject.FromObject(new {
                    taskIds = taskIdString
                }));
                replyText += $"Type the number of the task you want to log against {project.Name}";
            }

            return new
            {
                displayText = replyText,
                speech = replyText,
                contextOut = new[] { context }
            };
        }
    }
}