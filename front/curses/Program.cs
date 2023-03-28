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
    // private TcpClient _client;
    private bool _active = true;
    private nint _screen;
    private int SHeight { get; }
    private int SWidth { get; }
    private MatchState _lastState = MatchState.From(File.ReadAllText("state_test.json"));

    private float HOR_RATIO = 0.75f;

    private int _cardHeight;


    private PlayerInfoTab _topInfoTab;
    private Tab[] _topTabs;

    private PlayerInfoTab _bottomInfoTab;
    private Tab[] _bottomTabs;

    private int _curTabI = 0;

    bool _topOrBottom = false;
    public bool TopOrBottom { 
        get => _topOrBottom; 
        set {
            _curTabI = 0;
            _topOrBottom = value;
        } 
    }


    public CursesClient(string host, int port) {
        _topInfoTab = new(this, "Info", true);
        _topTabs = new Tab[] {
            _topInfoTab,
        };

        _bottomInfoTab = new(this, "Info", false);
        _bottomTabs = new Tab[] {
            _bottomInfoTab,
        };
        // var endpoint = new IPEndPoint(IPAddress.Parse(host), 8080);
        // _client = new TcpClient();

        // _client.Connect(endpoint);
        // var stream = _client.GetStream();

        _screen = NCurses.InitScreen();
        int sy, sx;
        NCurses.GetMaxYX(_screen, out sy, out sx);
        SHeight = sy;
        SWidth = sx;
        _cardHeight = SHeight / 2 - 1;
        NCurses.InitScreen();
        // NCurses.NoDelay(_screen, true);
        NCurses.NoEcho();
        NCurses.SetCursor(0);
    }

    public void Start() {
        try {
            while (_active) {
                Draw();
                Input();
                // System.Console.WriteLine("Reading message");
                // var prompt = Read();
                // if (prompt == null || prompt == "") break;
                // NCurses.MoveAddString(0, 0, $"Size: {_sHeight} {_sWidth}");
                // NCurses.MoveAddString(1, 1, $"Prompt: {prompt}");
                // var stateJ = Read();
                // var state = MatchState.From(stateJ);
                
                // NCurses.MoveAddString(2, 1, "Enter response: ");
                // NCurses.Refresh();

                // // System.Console.WriteLine("State parsed");
                // Write();
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

    void Draw() {
        DrawPlayArea();
        DrawSelectedCard();
        DrawLog();
    }

    void DrawPlayArea() {
        // TODO cant place in right bottom corner
        int width = (int)Math.Ceiling(SWidth * HOR_RATIO);
        CUtil.Box(0, 0, SHeight-1, width);
        CUtil.HorBoxLine(SHeight/2, 0, width);
        _topInfoTab.Draw(_lastState.Players[0], SHeight / 2-1, 1, false);
        _bottomInfoTab.Draw(_lastState.Players[0], SHeight / 2+1, 1, false);
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
    }

    static int TOP_CARD_BOX_WIDTH = 15;
    public void DrawTopCard(CardState card, int y, int x, bool drawCost, int diff) {
        CUtil.Box(y - 1 + diff, x, 3, TOP_CARD_BOX_WIDTH);
        NCurses.MoveAddString(y + diff, x + 1, card.Name);
    }
}

abstract class Tab {
    public CursesClient Parent { get; }
    public string Title { get; set; }

    public Tab(CursesClient parent, string title) {
        Title = title;
        Parent = parent;
    }

    abstract public void Draw(PlayerState state, int x, int y, bool selected);
}

class PlayerInfoTab : Tab
{  
    private bool _reverse;
    public PlayerInfoTab(CursesClient client, string title, bool reverse) : base(client, title)
    {
        _reverse = reverse;
    }

    public override void Draw(PlayerState player, int y, int x, bool selected)
    {
        var diff = _reverse ? -1 : 1;
        Parent.DrawTopCard(player.Bond, y, x, false, diff);
        y += 3*diff;
        NCurses.MoveAddString(y, x, player.Name);
        y += diff;
        NCurses.MoveAddString(y, x, $"Life: {player.Life}");
        y += diff;
        NCurses.MoveAddString(y, x, $"Energy: {player.Energy}");
        y += diff;
        NCurses.MoveAddString(y, x, $"Deck count: {player.DeckCount}");
        y += diff;
        NCurses.MoveAddString(y, x, $"Hand count: {player.HandCount}");
    }
}