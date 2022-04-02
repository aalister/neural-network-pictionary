using Microsoft.AspNetCore.SignalR;
using Timer = System.Timers.Timer;

namespace PictionaryAI
{
    public class Room
    {
        private readonly Dictionary<string, User> _connIdToUser;
        private Timer? _countdownTimer;
        private string[] _availablePrompts;
        private List<uint> previousPrompts = new List<uint>();

        public Room()
        {
            _connIdToUser = new Dictionary<string, User>();
            string newId = Guid.NewGuid().ToString();
            Id = newId.Substring(0, newId.IndexOf('-'));
            _availablePrompts = new string[] { "Aircraft Carrier", "Airplane", "Alarm Clock", "Ambulance", "Angel", "Animal Migration", "Ant", "Anvil", "Apple", "Arm", "Asparagus", "Axe", "Backpack", "Banana", "Bandage", "Barn", "Baseball", "Baseball Bat", "Basket", "Basketball", "Bat", "Bathtub", "Beach", "Bear", "Beard", "Bed", "Bee", "Belt", "Bench", "Bicycle", "Binoculars", "Bird", "Birthday Cake", "Blackberry", "Blueberry", "Book", "Boomerang", "Bottlecap", "Bowtie", "Bracelet", "Brain", "Bread", "Bridge", "Broccoli", "Broom", "Bucket", "Bulldozer", "Bus", "Bush", "Butterfly", "Cactus", "Cake", "Calculator", "Calendar", "Camel", "Camera", "Camouflage", "Campfire", "Candle", "Cannon", "Canoe", "Car", "Carrot", "Castle", "Cat", "Ceiling Fan", "Cello", "Cell Phone", "Chair", "Chandelier", "Church", "Circle", "Clarinet", "Clock", "Cloud", "Coffee Cup", "Compass", "Computer", "Cookie", "Cooler", "Couch", "Cow", "Crab", "Crayon", "Crocodile", "Crown", "Cruise Ship", "Cup", "Diamond", "Dishwasher", "Diving Board", "Dog", "Dolphin", "Donut", "Door", "Dragon", "Dresser", "Drill", "Drums", "Duck", "Dumbbell", "Ear", "Elbow", "Elephant", "Envelope", "Eraser", "Eye", "Eyeglasses", "Face", "Fan", "Feather", "Fence", "Finger", "Fire Hydrant", "Fireplace", "Firetruck", "Fish", "Flamingo", "Flashlight", "Flip Flops", "Floor Lamp", "Flower", "Flying Saucer", "Foot", "Fork", "Frog", "Frying Pan", "Garden", "Garden Hose", "Giraffe", "Goatee", "Golf Club", "Grapes", "Grass", "Guitar", "Hamburger", "Hammer", "Hand", "Harp", "Hat", "Headphones", "Hedgehog", "Helicopter", "Helmet", "Hexagon", "Hockey Puck", "Hockey Stick", "Horse", "Hospital", "Hot Air Balloon", "Hot Dog", "Hot Tub", "Hourglass", "House", "House Plant", "Hurricane", "Ice Cream", "Jacket", "Jail", "Kangaroo", "Key", "Keyboard", "Knee", "Knife", "Ladder", "Lantern", "Laptop", "Leaf", "Leg", "Light Bulb", "Lighter", "Lighthouse", "Lightning", "Line", "Lion", "Lipstick", "Lobster", "Lollipop", "Mailbox", "Map", "Marker", "Matches", "Megaphone", "Mermaid", "Microphone", "Microwave", "Monkey", "Moon", "Mosquito", "Motorbike", "Mountain", "Mouse", "Moustache", "Mouth", "Mug", "Mushroom", "Nail", "Necklace", "Nose", "Ocean", "Octagon", "Octopus", "Onion", "Oven", "Owl", "Paintbrush", "Paint Can", "Palm Tree", "Panda", "Pants", "Paper Clip", "Parachute", "Parrot", "Passport", "Peanut", "Pear", "Peas", "Pencil", "Penguin", "Piano", "Pickup Truck", "Picture Frame", "Pig", "Pillow", "Pineapple", "Pizza", "Pliers", "Police Car", "Pond", "Pool", "Popsicle", "Postcard", "Potato", "Power Outlet", "Purse", "Rabbit", "Raccoon", "Radio", "Rain", "Rainbow", "Rake", "Remote Control", "Rhinoceros", "Rifle", "River", "Roller Coaster", "Rollerskates", "Sailboat", "Sandwich", "Saw", "Saxophone", "School Bus", "Scissors", "Scorpion", "Screwdriver", "Sea Turtle", "See Saw", "Shark", "Sheep", "Shoe", "Shorts", "Shovel", "Sink", "Skateboard", "Skull", "Skyscraper", "Sleeping Bag", "Smiley Face", "Snail", "Snake", "Snorkel", "Snowflake", "Snowman", "Soccer Ball", "Sock", "Speedboat", "Spider", "Spoon", "Spreadsheet", "Square", "Squiggle", "Squirrel", "Stairs", "Star", "Steak", "Stereo", "Stethoscope", "Stitches", "Stop Sign", "Stove", "Strawberry", "Streetlight", "String Bean", "Submarine", "Suitcase", "Sun", "Swan", "Sweater", "Swing Set", "Sword", "Syringe", "Table", "Teapot", "Teddy-Bear", "Telephone", "Television", "Tennis Racquet", "Tent", "The Eiffel Tower", "The Great Wall Of China", "The Mona Lisa", "Tiger", "Toaster", "Toe", "Toilet", "Tooth", "Toothbrush", "Toothpaste", "Tornado", "Tractor", "Traffic Light", "Train", "Tree", "Triangle", "Trombone", "Truck", "Trumpet", "T-Shirt", "Umbrella", "Underwear", "Van", "Vase", "Violin", "Washing Machine", "Watermelon", "Waterslide", "Whale", "Wheel", "Windmill", "Wine Bottle", "Wine Glass", "Wristwatch", "Yoga", "Zebra", "Zigzag" };
        }

        public string Id { get; }
        public bool IsStarted { get; private set; }
        public bool IsRoundInProgress { get; private set; }

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
            User user = new User(connectionId, this, _connIdToUser.Count == 0, name);
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
                user.ChangeIsHost(false);
            }
            userToChange.ChangeIsHost(true);
        }

        public void StartGame(IHubContext<PictionaryHub> context, RoomManager roomManager, int countdownMillis)
        {
            IsStarted = true;
            _countdownTimer = new Timer(countdownMillis)
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

        public (string, uint) StartNewRound(IHubContext<PictionaryHub> context, RoomManager roomManager, int roundLengthMillis)
        {
            (string prompt, uint promptIndex) = GenerateNewPrompt();
            IsRoundInProgress = true;
            _countdownTimer = new Timer(roundLengthMillis)
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
            previousPrompts.Append(promptIndex);
            string prompt = _availablePrompts[promptIndex];
            return (prompt, promptIndex);
        }

        public void EndRound(IHubContext<PictionaryHub> context, RoomManager roomManager, int roundBreakMillis)
        {
            IsRoundInProgress = false;
            //The countdown timer might still be running if we're ending prematurely, so we need to stop it if it is
            StopTimer();
            //Now create a new countdown timer to start the next round
            _countdownTimer = new Timer(roundBreakMillis)
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
    }
}
