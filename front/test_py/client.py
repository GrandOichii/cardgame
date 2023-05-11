import random
from types import SimpleNamespace
import json


from front.test_py.frame import *
from front.test_py.sprites import *
# from frame import *
# from sprites import *


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

        # top = RectWidget(LGREEN)
        top = FormContainer()

        self.name_label = LabelWidget(ClientWindow.Instance.font, ' ')
        top.add_widget(self.name_label)

        self.health_label = LabelWidget(ClientWindow.Instance.font, '1')
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Health', RED), self.health_label)
        self.energy_label = LabelWidget(ClientWindow.Instance.font, 'loser')
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Energy', BLUE), self.energy_label)
        top.add_widget(RectWidget())

        self.add_widget(top)
        # card widget
        self.bond = CardWidget()
        self.add_widget(self.bond)
        # self.add_widget(RectWidget(BLACK, max_height=2))

    def load(self, data):
        self.name_label.set_text(data.name)
        self.health_label.set_text(str(data.life))
        self.energy_label.set_text(f'{data.energy} / {data.maxEnergy}')
        self.bond.load(data.bond)


class UnitContainer(VerContainer):
    def __init__(self):
        super().__init__()

        self.card = CardWidget()
        self.add_widget(self.card)

        self.attacks_label = LabelWidget(ClientWindow.Instance.font, '0', bg_color=LRED)
        self.add_widget(self.attacks_label)

    def load(self, data):
        attacks_text = ''
        card = None

        if data is not None:
            attacks_text = f'{data.attacksLeft}'
            card = data.card

        self.card.load(card)
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
        # self.lanes_container.add_widget(RectWidget())
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
            w += [RectWidget(WHITE, max_width=5)]
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
        self.bottom_player = PlayerContainer()
        self.hand = HandContainer()
        self.container = container

        mid_container = HorContainer()
        self.mid_label = LabelWidget(self.font, ' ')
        mid_container.add_widget(self.mid_label)
        mid_container.add_widget(RectWidget())

        sep = RectWidget(WHITE, max_height=10)
        container.add_widget(self.top_player)
        container.add_widget(sep)
        container.add_widget(self.bottom_player)
        container.add_widget(mid_container)
        container.add_widget(self.hand)
        container.add_widget(sep)

    def update(self):
        super().update()
        state = test_state()
        self.load(state)

    def load(self, state):
        self.top_player.load(state.players[0])
        self.bottom_player.load(state.players[1])
        self.hand.load(state.myData.hand)

        self.mid_label.set_text(f'({state.request}) {state.prompt}')
  