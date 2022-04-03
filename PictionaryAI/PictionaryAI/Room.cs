using Microsoft.AspNetCore.SignalR;
using Timer = System.Timers.Timer;

namespace PictionaryAI
{
    public class Room
    {
        private uint _nextUserId;
        private readonly Dictionary<string, User> _connIdToUser;
        private Timer? _countdownTimer;
        private DateTime _timeAtRoundStart;
        private readonly int _countdownMillis;
        private readonly int _roundLengthMillis;
        private readonly int _roundBreakMillis;
        private string[] _availablePrompts;
        private List<uint> previousPrompts = new List<uint>();

        public Room(int totalRounds, int countdownMillis, int roundLengthMillis, int roundBreakMillis)
        {
            _connIdToUser = new Dictionary<string, User>();
            string newId = Guid.NewGuid().ToString();
            Id = newId.Substring(0, newId.IndexOf('-'));
            TotalRounds = totalRounds;
            CurrentRound = 0;
            _countdownMillis = countdownMillis;
            _roundLengthMillis = roundLengthMillis;
            _roundBreakMillis = roundBreakMillis;
            _availablePrompts = new string[] { "Airplane", "Alarm Clock", "Anvil", "Apple", "Axe", "Baseball Bat", "Basketball", "Beard", "Bed", "Bench", "Bicycle", "Bird", "Book", "Bread", "Bridge", "Broom", "Butterfly", "Camera", "Candle", "Car", "Cat", "Ceiling Fan", "Cell Phone", "Chair", "Circle", "Clock", "Cloud", "Coffee Cup", "Cookie", "Cup", "Diving Board", "Donut", "Door", "Drums", "Dumbbell", "Envelope", "Eye", "Eyeglasses", "Face", "Fan", "Flower", "Frying Pan", "Grapes", "Hammer", "Hat", "Headphones", "Helmet", "Hot Dog", "Ice Cream", "Key", "Knife", "Ladder", "Laptop", "Light Bulb", "Lightning", "Line", "Lollipop", "Microphone", "Moon", "Mountain", "Moustache", "Mushroom", "Pants", "Paper Clip", "Pencil", "Pillow", "Pizza", "Power Outlet", "Radio", "Rainbow", "Rifle", "Saw", "Scissors", "Screwdriver", "Shorts", "Shovel", "Smiley Face", "Snake", "Sock", "Spider", "Spoon", "Square", "Star", "Stop Sign", "Suitcase", "Sword", "Syringe", "T-Shirt", "Table", "Tennis Racquet", "Tooth", "Traffic Light", "Tree", "Triangle", "Umbrella", "Wheel", "Wristwatch" };
        }

        public string Id { get; }
        public bool IsStarted { get; private set; }
        public bool IsRoundInProgress { get; private set; }
        public int TotalRounds { get; }
        public int CurrentRound { get; private set; }

        public bool ConnectionIdExists(string connectionId)
        {
            return _connIdToUser.ContainsKey(connectionId);
        }

        public User GetUserFromConnectionId(string connectionId)
        {
            if (!ConnectionIdExists(connectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            return _connIdToUser[connectionId];
        }

        public User[] GetUsers() => _connIdToUser.Values.ToArray();

        public bool IsEmpty() => _connIdToUser.Count == 0;

        public void DeleteRoom()
        {
            IsStarted = false;
            IsRoundInProgress = false;
            _connIdToUser.Clear();
            StopTimer();
        }

        private void StopTimer()
        {
            _countdownTimer?.Stop();
            _countdownTimer?.Dispose();
            _countdownTimer = null;
        }

        public User AddUser(string connectionId, string? name = null)
        {
            User user = new User(_nextUserId++, connectionId, this, _connIdToUser.Count == 0, name);
            _connIdToUser.Add(user.ConnectionId, user);
            return user;
        }

        public User RemoveUser(string connectionId)
        {
            User userToRemove = GetUserFromConnectionId(connectionId);
            if (_connIdToUser.Count > 1)
            {
                //Change host to someone else before we leave
                ChangeHost(_connIdToUser.Values.First(user => user.ConnectionId != userToRemove.ConnectionId));
            }
            _connIdToUser.Remove(connectionId);
            return userToRemove;
        }

        public void ChangeHost(User userToChange)
        {
            if (!ConnectionIdExists(userToChange.ConnectionId))
            {
                throw new ArgumentException("Connection id does not exist");
            }
            foreach (User user in _connIdToUser.Values)
            {
                user.IsHost = false;
            }
            userToChange.IsHost = true;
        }

        public void StartGame(IHubContext<PictionaryHub> context, RoomManager roomManager)
        {
            IsStarted = true;
            _countdownTimer = new Timer(_countdownMillis)
            {
                AutoReset = false,
                Enabled = true
            };
            _countdownTimer.Start();
            _countdownTimer.Elapsed += async (s, e) =>
            {
                StopTimer();
                await roomManager.StartNewRound(context, Id);
            };
        }

        public (string, uint) StartNewRound(IHubContext<PictionaryHub> context, RoomManager roomManager)
        {
            (string prompt, uint promptIndex) = GenerateNewPrompt();
            CurrentRound += 1;
            foreach (User user in _connIdToUser.Values)
            {
                user.HasCompletedDrawing = false;
            }
            _timeAtRoundStart = DateTime.UtcNow;
            IsRoundInProgress = true;
            _countdownTimer = new Timer(_roundLengthMillis)
            {
                AutoReset = false,
                Enabled = true
            };
            _countdownTimer.Start();
            _countdownTimer.Elapsed += async (s, e) =>
            {
                StopTimer();
                //Force-end the round
                await roomManager.EndRound(context, Id);
            };
            return (prompt, promptIndex);
        }

        private (string, uint) GenerateNewPrompt()
        {
            uint promptIndex = uint.MaxValue;
            while (promptIndex == uint.MaxValue || previousPrompts.Contains(promptIndex))
            {
                promptIndex = (uint)RandomContainer.GetRandomInRange(0, _availablePrompts.Length - 1);
            }
            if (previousPrompts.Count >= _availablePrompts.Length / 2)
            {
                previousPrompts.RemoveAt(0);
            }
            previousPrompts.Add(promptIndex);
            string prompt = _availablePrompts[promptIndex];
            return (prompt, promptIndex);
        }

        public void EndRound(IHubContext<PictionaryHub> context, RoomManager roomManager)
        {
            IsRoundInProgress = false;
            //The countdown timer might still be running if we're ending prematurely, so we need to stop it if it is
            StopTimer();
            //If we have to end the game, don't start the timer
            if (CurrentRound < TotalRounds)
            {
                //Now create a new countdown timer to start the next round
                _countdownTimer = new Timer(_roundBreakMillis)
                {
                    AutoReset = false,
                    Enabled = true
                };
                _countdownTimer.Start();
                _countdownTimer.Elapsed += async (s, e) =>
                {
                    StopTimer();
                    //Start a new round
                    await roomManager.StartNewRound(context, Id);
                };
            }
            else
            {
                IsStarted = false;
            }
        }

        public async Task PlayerCompletedDrawing(IHubContext<PictionaryHub> context, RoomManager roomManager, string connectionId)
        {
            User user = GetUserFromConnectionId(connectionId);
            user.AddScoreForCompletingDrawing((int)(DateTime.UtcNow - _timeAtRoundStart).TotalMilliseconds, _roundLengthMillis);
            //If everyone in the room is finished, end the round early
            if (_connIdToUser.Values.All(user => user.HasCompletedDrawing))
            {
                await roomManager.EndRound(context, Id);
            }
        }
    }
}
