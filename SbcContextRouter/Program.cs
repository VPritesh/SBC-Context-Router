using SbcContextRouter.Services;
using System.Net;
using System.Net.Sockets;

namespace SbcContextRouter
{
    class Program
    {
        private static readonly ContextResolver _resolver = new();
        private static readonly SessionContextManager _sessionManager = new();

        static async Task Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Loopback, 4573);
            listener.Start();
            Console.WriteLine("[C# Sidecar] SBC Context Router FastAGI server running on port 4573...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleFastAGIClientAsync(client));
            }
        }

        private static async Task HandleFastAGIClientAsync(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            string callerId = string.Empty;
            string calledNum = string.Empty;
            string callId = string.Empty;

            string line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
            {
                if (line.StartsWith("agi_callerid:"))
                    callerId = line.Split(':')[1].Trim();
                if (line.StartsWith("agi_extension:"))
                    calledNum = line.Split(':')[1].Trim();
                if (line.StartsWith("agi_arg_1:"))
                    callId = line.Split(':')[1].Trim();
            }

            var (accountId, profile, isFallback, version) = await _resolver.ResolveContextAsync(callerId, latencyBudgetMs: 5);

            var sessionContext = _sessionManager.GetOrEstablishDialogContext(
                callId: string.IsNullOrEmpty(callId) ? "DEFAULT-CALL-ID" : callId,
                fromTag: "local",
                toTag: "remote",
                accountId: accountId,
                profile: profile,
                isFallback: isFallback,
                version: version
            );

            await writer.WriteLineAsync($"SET VARIABLE CACHED_ACCOUNT_ID \"{sessionContext.AccountId}\"");
            await reader.ReadLineAsync();

            await writer.WriteLineAsync($"SET VARIABLE ROUTE_PROFILE \"{sessionContext.RouteProfile}\"");
            await reader.ReadLineAsync();

            string status = sessionContext.IsFallback ? "FALLBACK" : "RESOLVED";
            await writer.WriteLineAsync($"SET VARIABLE CONTEXT_STATUS \"{status}\"");
            await reader.ReadLineAsync();

            client.Close();
        }
    }
}
