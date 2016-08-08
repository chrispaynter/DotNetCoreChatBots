using System.Collections.Generic;

namespace DotNetCoreChatBots
{
    public class LogTimeChatBotUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FacebookId { get; set; }
        public string Email { get; set; }
    }
    public class LogTimeChatBotOptions
    {
        public List<LogTimeChatBotUser> Users { get; set; }
    }
}