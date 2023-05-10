from frame import *
from sprites import *
import random
from types import SimpleNamespace
import json


def test_state():
    try:
        return json.loads(open('state_test1.json', 'r').read(), object_hook=lambda d: SimpleNamespace(**d))
    except:
        return json.loads(open('front/test_py/state_test1.json', 'r').read(), object_hook=lambda d: SimpleNamespace(**d))
    

CHARS = 'abcdefghikklmnopqrstuvwxyz1234567890?!-+='
def random_string():
    size = random.randint(3, 10)
    result = ''
    for i in range(size):
        result += CHARS[random.randint(0, len(CHARS)-1)]
    return result


class InfoContainer(VerContainer):
    def __init__(self, parent: 'PlayerContainer', outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.pc_parent = parent

        top = FormContainer()

        self.name_label = LabelWidget(ClientWindow.Instance.font, ' ')
        top.add_widget(self.name_label)

        self.health_label = LabelWidget(ClientWindow.Instance.font, ' ')
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Health', RED), self.health_label)
        self.energy_label = LabelWidget(ClientWindow.Instance.font, 'loser')
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Energy', BLUE), self.energy_label)
        top.add_widget(RectWidget())

        self.add_widget(top)
        # card widget
        self.add_widget(RectWidget(RED, max_height=4+CARD_HEIGHT, max_width=4+CARD_WIDTH))

    def load(self, data):
        self.name_label.set_text(data.name)
        self.health_label.set_text(str(data.life))
        self.energy_label.set_text(f'{data.energy} / {data.maxEnergy}')


class UnitContainer(VerContainer):
    def __init__(self):
        super().__init__()


        self.card = CardWidget()
        self.add_widget(self.card)

        self.attacks_label = LabelWidget(ClientWindow.Instance.font, '0', bg_color=LRED)
        self.add_widget(self.attacks_label)

    def load(self, data):
        attacks_text = ''

        if data is not None:
            self.card.load(data.card)
            attacks_text = f'{data.attacksLeft}'

        self.attacks_label.set_text(attacks_text)


class LanesContainer(VerContainer):
    def __init__(self, parent: 'PlayerContainer', outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.pc_parent = parent
        self.units: list[UnitContainer] = []

        # TODO should be done with match config
        self.set_up_lanes(3)

    def set_up_lanes(self, lane_count: int):
        self.lanes_container = HorContainer()
        # sep = RectWidget(BLACK, max_width=1)
        sep = RectWidget()
        self.lanes_container.add_widget(sep)
        for i in range(lane_count):
            u = UnitContainer()
            self.units += [u]
            self.lanes_container.add_widget(u)
            self.lanes_container.add_widget(sep)
        self.lanes_container.add_widget(RectWidget())
        self.add_widget(RectWidget())
        self.add_widget(self.lanes_container)
        self.add_widget(RectWidget())

    def load(self, data):
        for i, unit in enumerate(self.units):
            unit.load(data[i])


class PlayerContainer(HorContainer):
    def __init__(self):
        super().__init__(RED)

        self.info_c = InfoContainer(self)
        self.add_widget(self.info_c)

        self.lanes_c = LanesContainer(self)
        self.add_widget(self.lanes_c)

    def load(self, data):
        self.info_c.load(data)
        self.lanes_c.load(data.units)


# TODO utilize
BETWEEN_CARDS = 2
class HandContainer(HorContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.start_widget = RectWidget(max_width=0, min_height=4+CARD_HEIGHT, max_height=4+CARD_HEIGHT)
        self.end_widget = RectWidget()

        self.set_widgets([])

    def set_widgets(self, widgets: list[Widget]):
        self.widgets.clear()
        self.widgets += [self.start_widget]
        self.widgets += widgets
        self.widgets += [self.end_widget]

    def load(self, data):
        w = []
        for card in data:
            w += [CardWidget.from_card(card)]
        self.set_widgets(w)


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
        self.top_player = PlayerContainer()
        container.add_widget(self.top_player)
        self.bottom_player = PlayerContainer()
        container.add_widget(self.bottom_player)
        self.hand = HandContainer()
        container.add_widget(self.hand)
        self.container = container

    def update(self):
        super().update()
        state = test_state()
        self.top_player.load(state.players[1])
        self.bottom_player.load(state.players[0])
        self.hand.load(state.myData.hand)
        
ClientWindow.Instance = ClientWindow()

ClientWindow.Instance.run()


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