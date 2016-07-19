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
    
    public class WitSessionHelper
    {
        private List<WitSession> _sessions = new List<WitSession>();

        public WitSession FindByFacebookUserId(string senderId)
        {
            return _sessions.FirstOrDefault(u => u.FacebookSenderId.Equals(senderId));
        }

        public WitSession FindBySessionId(string sessionId)
        {
            return _sessions.FirstOrDefault(u => u.WitSessionId.Equals(sessionId));
        }

        public WitSession FindOrCreateSession(string facebookSenderId)
        {
            var session = FindByFacebookUserId(facebookSenderId);

            if(session != null)
            {
                return session;
            }

            var sessionId = Guid.NewGuid().ToString("N");
            session = new WitSession(facebookSenderId, sessionId);
            _sessions.Add(session);
            
            return session;
        }

        public void EndSession(WitSession session)
        {
            _sessions.Remove(session);
        }
    }

}