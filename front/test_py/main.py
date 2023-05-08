from frame import *
import random

CHARS = 'abcdefghikklmnopqrstuvwxyz1234567890?!-+='
def random_string():
    size = random.randint(3, 10)
    result = ''
    for i in range(size):
        result += CHARS[random.randint(0, len(CHARS)-1)]
    return result


class InfoContainer(VerContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        top = FormContainer()

        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Hello'), LabelWidget(ClientWindow.Instance.font, ', world'))
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Your a'), LabelWidget(ClientWindow.Instance.font, 'loser'))

        self.add_widget(top)
        self.add_widget(RectWidget())


class PlayerContainer(HorContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(RED)

        self.add_widget(InfoContainer())
        # TODO
        self.add_widget(RectWidget())


class ClientWindow(Window):
    def __init__(self):
        ClientWindow.Instance = self

        super().__init__()

        self.init_ui()
        self.set_title('client test')


    def init_ui(self):
        # TODO remove, for easier debugging
        self.font = None
        try:
            ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 12)
        except:
            ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 12)

        container = VerContainer()
        container.add_widget(PlayerContainer())
        container.add_widget(PlayerContainer())
        self.container = container

        

    def draw(self):

        return super().draw()







w = ClientWindow()
w.run()


# def random_rect():
#     return RectWidget(random_color(), min_width=random.randint(10, 100), min_height=random.randint(10, 100))

# PREV_COLOR = [0, 255, 255]
# def next_color():
#     # return random_color()
#     global PREV_COLOR
#     PREV_COLOR[1] -= 10
#     PREV_COLOR[2] -= 10
#     return (PREV_COLOR[0], PREV_COLOR[1], PREV_COLOR[2])

# def add(c: Container, level: int):
#     w = RectWidget(next_color())
#     # w = random_widget()
#     # c.add_widget(w)
#     if level == 0:
#         return None
    
#     new = HorContainer()
#     # new = HorContainer1()
#     if level % 2 == 0:
#         new = VerContainer()
#         # new = VerContainer1()
#     c.add_widget(new)
#     new.add_widget(w)
#     add(new, level-1)
#     if level % 4 == 0 or level % 4 == 1:
#         new.widgets = new.widgets[::-1]

#     # new.add_widget(w)

# # add(c, 15)
# # w.container.widgets[0].move(50, 50)

# # TODO remove, for easier debugging
# font = None
# try:
#     ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
#     font = ContentPool.Instance.get_font('basic', 12)
# except:
#     ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
#     font = ContentPool.Instance.get_font('basic', 12)


# def click():
#     print('AA')


# def top_click():
#     print('top')

# def bottom_click():
#     print('bottom')

# container = VerContainer()
# container.move(10, 10)

# entries_count = 3

# # TODO don't fit, fix
# top_button = ButtonWidget(bg_color=LGREEN, max_height=20)
# top_button.click = top_click

# container.add_widget(top_button)
# for i in range(entries_count):
#     c = HorContainer()
#     left = RectWidget(LBLUE, max_width=20, max_height=20)
#     r = 'a'*(5 * (i+1))
#     r = 'a'*(random.randint(1, 20))
#     # r = 'aaa'
#     right = LabelWidget(font, r, bg_color=LRED)
#     # right = RectWidget(bg_color=RED)
#     c.add_widget(left)
#     c.add_widget(right)
#     # TODO doesn't work, loops
#     container.add_widget(RectWidget(BLACK, max_height=1))
#     container.add_widget(c)
# container.add_widget(RectWidget(BLACK, max_height=1))

# down_button = ButtonWidget(bg_color=LGREEN, max_height=20)
# down_button.click = bottom_click
# container.add_widget(down_button)
# container.move(200, 200)
# # w.container.add_widget(container)