using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PictionaryAI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly RoomManager _roomManager;
        private readonly PictionaryHub _hub;

        public GameController(RoomManager roomManager, PictionaryHub hub)
        {
            _roomManager = roomManager;
            _hub = hub;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Host([FromQuery(Name = "id")] string connectionId)
        {
            Room room = _roomManager.CreateRoom();
            User user = room.AddUser(connectionId);
            await _hub.Groups.AddToGroupAsync(user.ConnectionId, room.Id);
            return Ok(room.Id);
        }

        [HttpPost]
        public async Task<ActionResult> Join([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "code")] string roomId)
        {
            if (!_roomManager.RoomIdExists(roomId))
            {
                return NotFound();
            }
            Room room = _roomManager.GetRoomFromRoomId(roomId);
            User user = room.AddUser(connectionId);
            await _hub.Groups.AddToGroupAsync(user.ConnectionId, room.Id);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> SetName([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "name")] string name)
        {
            if (!_roomManager.ConnectionIdExists(connectionId))
            {
                return NotFound();
            }
            Room room = _roomManager.GetRoomFromConnectionid(connectionId);
            User user = room.GetUserFromConnectionid(connectionId);
            user.ChangeName(name);
            return Ok();
        }
    }
}
