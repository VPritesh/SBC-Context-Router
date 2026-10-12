using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SbcContextRouter.Services
{
    public record SessionContextRecord(string CallId, string AccountId, string RouteProfile, bool IsFallback, int Version);

    public class SessionContextManager
    {
        private readonly ConcurrentDictionary<string, SessionContextRecord> _activeDialogs = new();

        public SessionContextRecord GetOrEstablishDialogContext(string callId, string fromTag, string toTag, string accountId, string profile, bool isFallback, int version)
        {
            string dialogKey = $"{callId}:{fromTag}:{toTag}";

            return _activeDialogs.GetOrAdd(dialogKey, _ => new SessionContextRecord(
                CallId: callId,
                AccountId: accountId,
                RouteProfile: profile,
                IsFallback: isFallback,
                Version: version
            ));
        }
    }
}
