using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace DotNetCoreChatBots
{
    public class WitSession
    {
        public WitSession(string facebookSenderId, string witSessionId)
        {
            FacebookSenderId = facebookSenderId;
            WitSessionId = witSessionId;
            Context = new ExpandoObject();
        }
        public string FacebookSenderId { get; set; }
        public string WitSessionId { get; set; }
        public dynamic Context { get; set; }
    }
    
    public static class WitSessionHelper
    {
        private static List<WitSession> _sessions = new List<WitSession>();

        public static WitSession GetSession(string senderId)
        {
            return _sessions.FirstOrDefault(u => u.FacebookSenderId.Equals(senderId));
        }

        public static WitSession CreateSession(string senderId)
        {
            var session = GetSession(senderId);

            if(session != null)
            {
                EndSession(session);
            }

            var sessionId = Guid.NewGuid().ToString("N").Substring(0, 5);
            session = new WitSession(senderId, sessionId);
            _sessions.Add(session);
            
            return session;
        }

        public static void EndSession(WitSession session)
        {
            _sessions.Remove(session);
        }
    }

}