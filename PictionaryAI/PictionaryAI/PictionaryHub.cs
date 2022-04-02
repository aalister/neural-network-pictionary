using Microsoft.AspNetCore.SignalR;

namespace PictionaryAI
{
    public class PictionaryHub : Hub
    {
        public async Task Ping(string test)
        {
            await Clients.Caller.SendAsync("Pong", test);
        }
    }
}
