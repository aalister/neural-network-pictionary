using Microsoft.AspNetCore.SignalR;

namespace PictionaryAI
{
    public class PictionaryHub : Hub
    {
        private readonly RoomManager _roomManager;
        private readonly IHubContext<PictionaryHub> _context;

        public PictionaryHub(RoomManager roomManager, IHubContext<PictionaryHub> context)
        {
            _roomManager = roomManager;
            _context = context;
        }

        public async Task Ping(string test)
        {
            await Clients.Caller.SendAsync("Pong", test);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            await base.OnDisconnectedAsync(exception);
            if (_roomManager.ConnectionIdExists(connectionId))
            {
                Room room = _roomManager.GetRoomFromConnectionId(connectionId);
                await _roomManager.RemoveUser(_context, connectionId);
            }
        }
    }
}
