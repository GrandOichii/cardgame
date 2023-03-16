using Mindmagma.Curses;

class CommonUtil {
    static public List<String> BreakText(string text, int maxWidth) {
        var result = new List<String>();
        if (text == "") return result;

        var words = text.Split(' ');
        var line = "";
        foreach (var word in words) {
            if ((line + word + " ").Length >= maxWidth) {
                result.Add(line);
                line = word + " ";
                continue;
            }
            line += word + " ";
        }
        result.Add(line);
        return result;
    }
}

class NCUtil {
    static public void Box(int y, int x, int height, int width) {
        height-=1;
        width-=1;

        NCurses.MoveAddChar(y, x, CursesLineAcs.ULCORNER);
        NCurses.MoveAddChar(y, x+width, CursesLineAcs.URCORNER);
        NCurses.MoveAddChar(y+height, x, CursesLineAcs.LLCORNER);
        NCurses.MoveAddChar(y+height, x+width, CursesLineAcs.LRCORNER);

        for (int i = y+1; i < y+height; i++) {
            NCurses.MoveAddChar(i, x, CursesLineAcs.VLINE);
            NCurses.MoveAddChar(i, x+width, CursesLineAcs.VLINE);
        }
        for (int i = x+1; i < x+width; i++) {
            NCurses.MoveAddChar(y, i, CursesLineAcs.HLINE);
            NCurses.MoveAddChar(y+height, i, CursesLineAcs.HLINE);
        }
    }

    static public void HorBoxLine(int y, int x, int width) {
        width-=1;
        NCurses.MoveAddChar(y, x, CursesLineAcs.LTEE);
        NCurses.MoveAddChar(y, x+width, CursesLineAcs.RTEE);
        for (int i = x+1; i < x+width; i++)
            NCurses.MoveAddChar(y, i, CursesLineAcs.HLINE);
    }
}

class DrawableCard : Card {
    static public readonly int LOWER_CARD_HEIGHT = 20;
    static public readonly int LOWER_CARD_WIDTH = 30;

    public DrawableCard(string name, string text, string typeLine) : base(name, typeLine, text) {
    }

    public void Draw(int y, int x) {
        NCUtil.Box(y, x, DrawableCard.LOWER_CARD_HEIGHT, DrawableCard.LOWER_CARD_WIDTH);

        NCurses.MoveAddString(y+1, x+1, Name);
        NCUtil.HorBoxLine(y+2, x, DrawableCard.LOWER_CARD_WIDTH);        

        NCurses.MoveAddString(y+3, x+1, TypeLine);
        NCUtil.HorBoxLine(y+4, x, DrawableCard.LOWER_CARD_WIDTH);        

        int c = 0;
        foreach (var tt in Text.Split('\n')) {
            var lines = CommonUtil.BreakText(tt, LOWER_CARD_WIDTH-2);
            foreach (var line in lines) {
                NCurses.MoveAddString(y+5+c, x+1, line);
                c++;
            } 
            c++;
        }
    }

    public void DrawTop(int y, int x, bool selected=false) {
        if (selected) {
            NCurses.AttributeSet(NCurses.ColorPair(1));
            // NCurses.AttributeSet();
            // NCurses.Background()
        }
        NCUtil.Box(y, x, 3, DrawableCard.LOWER_CARD_WIDTH);
        NCurses.AttributeSet(CursesAttribute.NORMAL);
        NCurses.MoveAddString(y+1, x+1, Name);

    }
}

class Program {
    static nint Screen;


    static void Main(string[] args)
    {
        Screen = NCurses.InitScreen();

        try {
            Task();
        }
        finally {
            NCurses.EndWin();
        }
    }

    static void Task() {
        // NCurses.NoDelay(Screen, true);
        NCurses.NoEcho();
        NCurses.Keypad(Screen, true);
        NCurses.SetCursor(0);
        NCurses.StartColor();

        NCurses.InitPair(1, CursesColor.GREEN, CursesColor.BLACK);

        var c1 = new DrawableCard("Ascension Altar", "Fort", "Destroy one of your creatures: {Power up} this card.\nWhen (0/ 0 /7) is equal to 7, destroy it and put all creatures and forts from you hand into play.\n[cardname] can't be {Powered Down}.");
        var c2 = new DrawableCard("Spiked floor", "Fort", "When [cardname] enters play, give it to your opponent.\nWhen a creature enters play under your control, [cardname] deals (0/ 1 /2) damage to that creature.");
        var c3 = new DrawableCard("Ascension Altar", "Fort", "Destroy one of your creatures: {Power up} this card.\nWhen (0/ 0 /7) is equal to 7, destroy it and put all creatures and forts from you hand into play.\n[cardname] can't be {Powered Down}.");
        var cards = new List<DrawableCard>() {c1, c2, c3};
        int curI = 0;
        // c2.Draw(1, 1);
        // c2.DrawTop(1, 32, true);
        // NCurses.Keypad(Screen, true);
        int input;
        do {
            // draw
            // draw tops of cards
            cards[curI].Draw(0, 0);

            int y = 0;
            int x = DrawableCard.LOWER_CARD_WIDTH+1;
            for (int i = 0; i < cards.Count; i++) {
                var card = cards[i];
                card.DrawTop(y, x);
                y += 2;
            }
            y=0;
            for (int i = 1; i < cards.Count; i++) {
                y += 2;
                NCUtil.HorBoxLine(y, x, DrawableCard.LOWER_CARD_WIDTH);
            }
            int selY = curI*2;
            cards[curI].DrawTop(selY, x, true);
            NCurses.AttributeSet(NCurses.ColorPair(1));
            if (curI != 0)
                NCUtil.HorBoxLine(selY, x, DrawableCard.LOWER_CARD_WIDTH);
            if (curI != cards.Count-1)
                NCUtil.HorBoxLine(selY+2, x, DrawableCard.LOWER_CARD_WIDTH);
            NCurses.AttributeSet(CursesAttribute.NORMAL);


            // input
            input = NCurses.GetChar();
            if (input == CursesKey.UP) {
                curI--;
                if (curI < 0)
                    curI = cards.Count-1;
            }
            if (input == CursesKey.DOWN) {
                curI++;
                if (curI >= cards.Count)
                    curI = 0;
            }

            NCurses.Clear();
        }
        while (input != 'q');

    }
}