using System.Net;
using System.Net.Sockets;
using System.Text;
using Mindmagma.Curses;
using Shared;

class Client {
    static int BUFFER_SIZE = 4096;
    static byte[] buffer = new byte[BUFFER_SIZE];
    static TcpClient client = new TcpClient();


    static void Main(string[] args)
    {
        var host = "127.0.0.1";
        if (args.Length == 1) host = args[0];
        int port = 8080;

        var c = new CursesClient(host, port);
        c.Start();
    }

}


class CursesClient {
    private TcpClient _client;
    private bool _active = true;
    private nint _screen;
    private int SHeight { get; }
    private int SWidth { get; }
    private MatchState _lastState;


    private List<Tab> Tabs = new();
    private int _curTabI = 0;

    private TabCluster TopTabs;
    private TabCluster BottomTabs;
    private bool _topSelected = false;


    public CursesClient(string host, int port) {
        // _topInfoTab = new(this, "Info", true);
        // _topTabs = new Tab[] {
        //     _topInfoTab,
        // };

        // _bottomInfoTab = new(this, "Info", false);
        // _bottomTabs = new Tab[] {
        //     _bottomInfoTab,
        // };
        var endpoint = new IPEndPoint(IPAddress.Parse(host), 8080);
        _client = new TcpClient();

        _client.Connect(endpoint);
        var stream = _client.GetStream();

        _screen = NCurses.InitScreen();
        int sy, sx;
        NCurses.GetMaxYX(_screen, out sy, out sx);
        SHeight = sy;
        SWidth = sx;
        // _cardHeight = SHeight / 2 - 1;
        NCurses.InitScreen();
        NCurses.NoEcho();
        NCurses.SetCursor(0);

        var half = SHeight/2;

        TopTabs = new(half, SWidth);
        // TopTabs.Tabs.Add(new TestTab("Test", 0, 0, ))
        BottomTabs = new(half, SWidth);
    }

    public void Start() {
        try {
            while (_active) {
                System.Console.WriteLine("Reading message");
                var prompt = Read();
                if (prompt == null || prompt == "") break;
                // NCurses.MoveAddString(0, 0, $"Size: {_sHeight} {_sWidth}");
                NCurses.MoveAddString(1, 1, $"Prompt: {prompt}");
                var stateJ = Read();
                var state = MatchState.From(stateJ);

                // _lastState = MatchState.From(File.ReadAllText("../state_test.json"));
                // LoadState();
                // Draw();
                // Input();

                
                NCurses.MoveAddString(2, 1, "Enter response: ");
                NCurses.Refresh();

                // System.Console.WriteLine("State parsed");
                Write();
                NCurses.Clear();
            }
        } catch (Exception ex) {
            Close();
            System.Console.WriteLine(ex);
        } finally {
            Close();
        }
    }

    void Close() {
        // NCurses.Echo();
        NCurses.EndWin();
        // _client.Close();
    }

    string Read() {
        // var stream = _client.GetStream();
        // var message = NetUtil.Read(stream);
        // return message;
        return "";
    }

    void Write() {
        // var stream = _client.GetStream();
        // var message = Console.ReadLine();
        var message = "pass";
        if (message is null) message = "";

        // NetUtil.Write(stream, message);
    }

    void LoadState() {
        // pass the payload to each respective tab cluster
    }

    void Draw() {
        // DrawPlayArea();
        // DrawSelectedCard();
        // DrawLog();

        // TopTabs.Draw(_topSelected);
        // BottomTabs.Draw(!_topSelected);
    }

    void DrawSelectedCard() {

    }

    void DrawLog() {
        // TODO

    }

    void Input() {
        var key = NCurses.GetChar();
        if (key == 'q') {
            _active = false;
            return;
        }
        if (key == ' ') {
            _topSelected = !_topSelected;
            return;
        }
        if (_topSelected)
            TopTabs.ProcessInput(key);
        else BottomTabs.ProcessInput(key);
    }

    static int TOP_CARD_BOX_WIDTH = 15;

    public void DrawTopCard(CardState card, int y, int x, bool drawCost, int diff) {
        CUtil.Box(y - 1 + diff, x, 3, TOP_CARD_BOX_WIDTH);
        NCurses.MoveAddString(y + diff, x + 1, card.Name);
    }

}

// abstract class Tab {
//     public CursesClient Parent { get; }
//     public string Title { get; set; }

//     public Tab(CursesClient parent, string title) {
//         Title = title;
//         Parent = parent;
//     }

//     abstract public void Draw(PlayerState state, int x, int y, bool selected);
// }

// class PlayerInfoTab : Tab
// {  
//     private bool _reverse;
//     public PlayerInfoTab(CursesClient client, string title, bool reverse) : base(client, title)
//     {
//         _reverse = reverse;
//     }

//     public override void Draw(PlayerState player, int y, int x, bool selected)
//     {
//         var diff = _reverse ? -1 : 1;
//         Parent.DrawTopCard(player.Bond, y, x, false, diff);
//         y += 3*diff;
//         NCurses.MoveAddString(y, x, player.Name);
//         y += diff;
//         NCurses.MoveAddString(y, x, $"Life: {player.Life}");
//         y += diff;
//         NCurses.MoveAddString(y, x, $"Energy: {player.Energy}");
//         y += diff;
//         NCurses.MoveAddString(y, x, $"Deck count: {player.DeckCount}");
//         y += diff;
//         NCurses.MoveAddString(y, x, $"Hand count: {player.HandCount}");
//     }
// }

abstract class IDrawable {
    protected int _height;
    protected int _width;

    public IDrawable(int height, int width) {
        
        _width = width;
        _height = height;

    }

    abstract public void ProcessInput(int input);

    abstract public void Draw(int y, int x, bool selected);
}

abstract class Tab : IDrawable {
    private string _title;

    // public int BorderColor { get; set; }=CursesColor.
    
    public Tab(string title, int height, int width) : base(height, width) {
        _title = title;
    }

    public override void Draw(int y, int x, bool selected)
    {
        // draw outline
        CUtil.Box(y, x, _height, _width);
        // draw title
        if (selected)
            NCurses.AttributeOn(CursesAttribute.BOLD);
        NCurses.MoveAddString(y, x + 1, _title);
        NCurses.AttributeOff(CursesAttribute.BOLD);
        // draw insides
        DrawInsides(y, x);
    }

    abstract public void DrawInsides(int y, int x);
}


class TestTab : Tab
{
    public TestTab(string title, int height, int width) : base(title, height, width)
    {
    }

    public override void DrawInsides(int y, int x)
    {
        // throw new NotImplementedException();
    }

    public override void ProcessInput(int input)
    {
        // throw new NotImplementedException();
    }
}


class TabCluster : IDrawable {
    private static int NEXT_TAB_KEY = CursesKey.BTAB;
    private static int PREV_TAB_KEY = CursesKey.STAB;

    public List<Tab> Tabs { get; }=new();
    private int _curTabI=0;
    public int CurTabI {
        get => _curTabI;
        set {
            _curTabI = value;
            if (_curTabI >= Tabs.Count) _curTabI = 0;
            if (_curTabI < 0) _curTabI = Tabs.Count - 1;
        }
    }

    public TabCluster(int height, int width) : base(height, width) {
    }

    public override void ProcessInput(int input)
    {
        if (input == NEXT_TAB_KEY) {
            CurTabI++;
            return;
        }
        
        if (input == PREV_TAB_KEY) {
            CurTabI--;
            return;
        }

        // INPUT NOT RECOGNIZED
    }

    public override void Draw(int y, int x, bool selected)
    {
        for (int i = 0; i < Tabs.Count; i++)
            Tabs[i].Draw(y, x, i == CurTabI);
    }
}