
using Mindmagma.Curses;

static class CUtil {
    // TODO
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

    static public void Box(int y, int x, int height, int width) {
        height-=1;
        width-=1;

        NCurses.MoveAddChar(y, x, CursesLineAcs.ULCORNER);
        NCurses.MoveAddChar(y, x+width, CursesLineAcs.URCORNER);
        NCurses.MoveAddChar(y+height, x, CursesLineAcs.LLCORNER);
        NCurses.MoveAddChar(y+height, x+width, CursesLineAcs.LRCORNER);
        // NCurses.Move(y+height, x+width);
        // NCurses.InsertChar(CursesLineAcs.LRCORNER);

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