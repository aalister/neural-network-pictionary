using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace PictionaryAI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly RoomManager _roomManager;
        private readonly IHubContext<PictionaryHub> _pictionaryHub;

        public GameController(RoomManager roomManager, IHubContext<PictionaryHub> pictionaryHub)
        {
            _roomManager = roomManager;
            _pictionaryHub = pictionaryHub;
        }

        [HttpPost]
        [Route("host")]
        [Route("host/{name}")]
        public async Task<ActionResult<string>> Host([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "name")] string? name = null)
        {
            Room room = _roomManager.CreateRoom();
            await _roomManager.AddUser(_pictionaryHub, room.Id, connectionId, name);
            return Ok(room.Id);
        }

        [HttpPost]
        [Route("join/{code}")]
        [Route("join/{code}/{name}")]
        public async Task<ActionResult> Join([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "code")] string roomId, [FromRoute(Name = "name")] string? name = null)
        {
            if (!_roomManager.RoomIdExists(roomId))
            {
                return NotFound();
            }
            await _roomManager.AddUser(_pictionaryHub, roomId, connectionId, name);
            return Ok();
        }

        [HttpPost]
        [Route("setname/{name}")]
        public async Task<ActionResult> SetName([FromQuery(Name = "id")] string connectionId, [FromRoute(Name = "name")] string name)
        {
            if (!_roomManager.ConnectionIdExists(connectionId))
            {
                return NotFound();
            }
            await _roomManager.ChangeUserName(_pictionaryHub, connectionId, name);
            return Ok();
        }

        [HttpPost]
        [Route("startgame")]
        public async Task<ActionResult> StartGame([FromQuery(Name = "id")] string connectionId)
        {
            if (!_roomManager.ConnectionIdExists(connectionId))
            {
                return NotFound();
            }
            Room room = _roomManager.GetRoomFromConnectionId(connectionId);
            User user = room.GetUserFromConnectionId(connectionId);
            if (!user.IsHost || room.IsStarted)
            {
                return Unauthorized();
            }
            //We're good to go, start the game
            await _roomManager.StartGame(_pictionaryHub, room.Id);
            return Ok();
        }
    }
}
