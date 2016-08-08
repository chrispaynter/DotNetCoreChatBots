using System;
using System.Linq;
using System.Threading.Tasks;
using Paynter.ApiAi.Models;
using Newtonsoft.Json.Linq;

namespace DotNetCoreChatBots
{
    public partial class LogTimeChatBotActions
    {
        public async Task<object> FindProject(ApiAiQueryResponse response)
        {
            //var user = GetUser(response);
            //if(user == null) return UnauthorisedMessage();

            var projectName = (string)response.Result.Parameters["project"];
            var allProjects = _harvestDataHelper.QueryProjectsByName(projectName).ToList();

            var replyText = "";
            var attempts = Convert.ToInt32((string)response.Result.Parameters["attempts"]);
            ApiAiQueryContext context = null;

            if (allProjects.Count == 0)
            {
                if (attempts == 1)
                {
                    replyText = $"So I looked for all projects that sound like '{projectName}', but can't seem to find any :( Can you try writing the name again, maybe in a different way?";
                }
                else if (attempts == 2)
                {
                    replyText = $"Sorry I couldn't find anything for that either :/ Maybe try writing it in another way?";
                }
                else
                {
                    replyText = $"No luck :( It might not yet be in Harvest, or possibly it's been mispelt. Might have to do this one manually, sorry about that!";
                }

                attempts++;
                context = new ApiAiQueryContext("project-not-found", JObject.FromObject(new { attempts = attempts }));

            }
            else if (allProjects.Count == 1)
            {
                var project = allProjects.FirstOrDefault();
                context = new ApiAiQueryContext("project-ask-single", JObject.FromObject(new { projectId = project.Id }));
                replyText = $"I found one project that matches. Do you mean [{project.Code}] {project.Name}?";
            }
            else if (allProjects.Count > 1)
            {
                replyText = $"Alright, I found {allProjects.Count} projects that you might have meant? ";

                var i = 1;
                foreach (var proj in allProjects)
                {
                    replyText += $"({i}) {proj.Name}, ";
                    i++;
                }
                replyText.TrimEnd(new[] { ' ', ',' });

                context = new ApiAiQueryContext("project-ask-multiple");
                replyText += "Type the number of the project you want to log against.";
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