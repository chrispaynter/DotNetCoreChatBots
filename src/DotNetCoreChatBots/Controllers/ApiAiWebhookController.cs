
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots.Controllers
{
    public class ApiAiWebhookController : Controller
    {
        private HarvestDataHelper _harvestDataHelper;

        public ApiAiWebhookController(HarvestDataHelper harvestDataHelper)
        {
            _harvestDataHelper = harvestDataHelper;
        }

        [HttpPost("api/aiapiwebhook")]
        public async Task<object> ApiAiWebhook([FromBody]ApiAiQueryResponse response)
        {
            // var passedAuthString = (string)Request.Headers["Authorisation"];
            // var expectedAuthString = System.Convert.ToBase64String();

            // if(expectedAuthString != passedAuthString)
            // {
            //     throw a 403
            // }

            if(response.Result.Action == "LogTime.FindProject")
            {
                var projectName = (string)response.Result.Parameters["project"];
                var task = await _harvestDataHelper.QueryProjectsByName(projectName);
                var allProjects = task.ToList();

                var replyText = "";
                var attempts = Convert.ToInt32((string)response.Result.Parameters["attempts"]);
                ApiAiQueryContext context = null;
                
                if(allProjects.Count == 0)
                {
                    if(attempts == 1)
                    {
                        replyText = $"So I looked for all projects that sound like '{projectName}', but can't seem to find any :( Can you try writing the name again, maybe in a different way?";
                    }
                    else if(attempts == 2)
                    {
                        replyText = $"Sorry I couldn't find anything for that either :/ Maybe try writing it in another way?";
                    }
                    else
                    {
                        replyText = $"No luck :( It might not yet be in Harvest, or possibly it's been mispelt. Might have to do this one manually, sorry about that!";
                    }

                    attempts++;
                    context = new ApiAiQueryContext("project-not-found", JObject.FromObject(new { attempts = attempts, test = "ASDFSDF" }));
                    
                }   
                else if(allProjects.Count == 1)
                {
                    var project = allProjects.FirstOrDefault();
                    context = new ApiAiQueryContext("project-ask-single");
                    replyText = $"I found one project that matches. Do you want to log time against {project.Name}?";
                }
                else if(allProjects.Count > 1)
                {
                    replyText = $"Alright, I found {allProjects.Count} projects that you might have meant? ";

                    var i = 1;
                    foreach(var proj in allProjects)
                    {
                        replyText += $"({i}) {proj.Name}, ";
                        i++;
                    }
                    replyText.TrimEnd(new []{' ', ','});

                    context = new ApiAiQueryContext("project-ask-multiple");
                    replyText += "Type the number of the project you want to log against.";
                }


                return new {
                    displayText = replyText,
                    speech = replyText,
                    contextOut = new [] { context } 
                };
            }




            
            var displayText = "";
            var contexts = new List<object>();

            if(response.Result.Action == "timesheet.findProject")
            {
                var projectName = (string)response.Result.Parameters["project"];
                var task = await _harvestDataHelper.QueryProjectsByName(projectName);
                var allProjects = task.ToList();


                var notFoundContext = new ApiAiQueryContext(){ Name = "project-not-found", Lifespan = 0 };
                var multipleFoundContext =  new ApiAiQueryContext() { Name = "project-multiple-found", Lifespan = 0 };
                var singleFoundContext =  new ApiAiQueryContext() { Name = "project-single-found", Lifespan = 0 };

                contexts.Add(notFoundContext);
                contexts.Add(multipleFoundContext);
                contexts.Add(singleFoundContext);

                if(allProjects.Count == 0)
                {
                    displayText = "I couldn't find a project that sounds like that. Could you try writing it another way?";
                    notFoundContext.Lifespan = 1;
                }
                else if(allProjects.Count == 1)
                {
                    var project = allProjects.FirstOrDefault();
                    displayText = $"Did you mean {project.Name}?";
                    singleFoundContext.Lifespan = 1;
                    singleFoundContext.Parameters = JObject.FromObject(new {
                        projectId = project.Id
                    });
                }
                else if(allProjects.Count > 1)
                {
                    var returnString = "";
                    var i = 1;
                    foreach(var proj in allProjects)
                    {
                        returnString += $"({i}) {proj.Name}, ";
                        i++;
                    }
                    returnString.TrimEnd(new []{' ', ','});

                    var idArray = allProjects.Select(u => u.Id).ToArray();
                    displayText = $"I found a few projects that you might mean. {returnString}. Type the number of the project you meant, or 'Not here' if the it's missing.";
                    multipleFoundContext.Lifespan = 1;
                    multipleFoundContext.Parameters = JObject.FromObject(new {
                        projectIds = idArray
                    });
                }

            }

            if(response.Result.Action == "timesheet.projectIndex")
            {
                var idResponseString = (string)response.Result.Parameters["projectIndex"];

                if(idResponseString == "not here")
                {

                }
                else
                {

                    var projectMultipleContext = response.Result.Contexts.FirstOrDefault(u => u.Name == "project-multiple-found");
                    var ids = projectMultipleContext.Parameters["projectIds"];

                    var projectIndex = Convert.ToInt32(idResponseString);

                    var projectId = Convert.ToString(ids[projectIndex - 1]);

                    var project = await _harvestDataHelper.GetProjectById(projectId);

                    contexts.Add(new ApiAiQueryContext{ Name = "project-found", Lifespan = 2, Parameters = JObject.FromObject(new {
                        projectId = projectId,
                        projectName = project.Name
                    } )});

                    displayText = $"Alright, let's log some time on {project.Name}. How long did you spend on it?";
                }
                
                
            }        

            if(response.Result.Action == "timesheet.logTime")
            {

            }    

            return new {
                displayText = displayText,
                speech = displayText,
                contextOut = contexts.ToArray()
            };
        }


    }
}