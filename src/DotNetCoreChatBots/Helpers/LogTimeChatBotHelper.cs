// using System.Threading.Tasks;
// using Paynter.ApiAi.Models;

// namespace DotNetCoreChatBots
// {
//     public class LogTimeChatBotHelper
//     {
//         private LogTimeChatBotActions _actions;

//         public LogTimeChatBotHelper(LogTimeChatBotActions actions)
//         {
//             _actions = actions;
//         }
//         public async Task<object> ProcessWebhook(ApiAiQueryResponse response)
//         {
//             switch (response.Result.Action)
//             {
//                 case "LogTime.FindProject":
//                     return await _actions.FindProject(response);
//                 case "LogTime.FindTasks":
//                     return await _actions.FindTasks(response);
//                 case "LogTime.ConfirmProject":
//                     return await _actions.ConfirmProject(response);
//                 case "LogTime.LogTime":
//                     return await _actions.LogTime(response);
//                     break;
//             }

//             return null;
//         }
//     }
// }