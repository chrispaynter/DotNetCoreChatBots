using System.Threading.Tasks;
using Paynter.ApiAi.Models;

namespace DotNetCoreChatBots
{
    public partial class LogTimeChatBotActions
    {
        public async Task<object> LogTime(ApiAiQueryResponse response)
        {
            var projectId = response.GetParameter("projectId");
            var timesheetDate = response.GetParameter("timesheetDate");
            var timeSpent = response.GetParameter("timeSpent");
            var description = response.GetParameter("description");
            
            var user = GetUser(response);
            var harvestUser = _harvestDataHelper.GetUserByEmail(user.Email);

            return null;
        }
    }
}