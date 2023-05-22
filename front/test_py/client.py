import socket
import random
from types import SimpleNamespace
import json

import pygame as pg


from front.test_py.frame import *
from front.test_py.frame import WHITE, ClickConfig, Rect, WindowConfigs
from front.test_py.sprites import *
# from frame import *
# from sprites import *


def parse_state(textj):
    return json.loads(textj, object_hook=lambda d: SimpleNamespace(**d))


def test_state():
    try:
        return parse_state(open('state_test.json', 'r').read())
    except:
        return parse_state(open('front/test_py/state_test.json', 'r').read())
    

CHARS = 'abcdefghikklmnopqrstuvwxyz1234567890?!-+='
def random_string():
    size = random.randint(3, 10)
    result = ''
    for i in range(size):
        result += CHARS[random.randint(0, len(CHARS)-1)]
    return result


class LogsContainer(ScrollWidget):
    def __init__(self):
        super().__init__(RED, min_height=200)
        self.container = VerContainer()
        self.container.add_widget(RectWidget(max_height=0))
        self.set_widget(self.container)

        self.fuze = True

    def load(self, data):
        for log in data:
            l = LabelWidget(ClientWindow.Instance.font, log)
            self.container.add_widget(l)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        diff = self.container.get_pref_height() - bounds.height
        print(self.container.get_pref_height(), bounds.height, self.container.get_pref_height() - bounds.height)
        if diff > 0:
            self.scroll = -diff
        return super().draw(surface, bounds, configs)


class NameLabelWidget(LabelWidget):
    def __init__(self, font):
        font = ClientWindow.Instance.font

        click_configs = []
        cc1 = ClickConfig()

        # TODO
        cc1.mouse_over_condition = lambda: client.ClientWindow.Instance.last_state.request == 'pick attack target'
        cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc1.mouse_click_condition = lambda: True
        cc1.mouse_click_action = self.pick_attack_action

        click_configs = [
            cc1
        ]

        super().__init__(font, '', click_configs=click_configs)

    def pick_attack_action(self):
        print('mogus')
        client.ClientWindow.Instance.send_response('player')


class InfoContainer(VerContainer):
    def __init__(self, parent: 'PlayerContainer', outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.pc_parent = parent

        # top = RectWidget(LGREEN)
        top = FormContainer()

        self.name_label = NameLabelWidget(ClientWindow.Instance.font)
        top.add_widget(self.name_label)

        self.health_label = LabelWidget(ClientWindow.Instance.font, ' ')
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Health', RED), self.health_label)
        self.energy_label = LabelWidget(ClientWindow.Instance.font, ' ')
        top.add_pair(LabelWidget(ClientWindow.Instance.font, 'Energy', BLUE), self.energy_label)
        top.add_widget(SPACE_FILLER)

        self.add_widget(top)
        # card widget
        self.bond = CardInPlayWidget()
        self.add_widget(self.bond)
        # self.add_widget(RectWidget(BLACK, max_height=2))

    def load(self, data):
        self.name_label.set_text(data.name)
        self.health_label.set_text(str(data.life))
        self.energy_label.set_text(f'{data.energy} / {data.maxEnergy}')
        self.bond.load(data.bond)


class UnitContainer(VerContainer):
    def __init__(self, lane_i: str):
        super().__init__()
        self.lane_i = lane_i

        self.last_attacks_left = 0

        self.card = UnitCardWidget(lane_i)
        self.add_widget(self.card)

        cc = ClickConfig()
        cc.mouse_over_condition = lambda: client.ClientWindow.Instance.last_state.request == 'enter command' and self.last_attacks_left > 0
        cc.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, BLUE, (bounds.x, bounds.y, bounds.width, bounds.height), 1)
        cc.mouse_click_condition = lambda: True
        cc.mouse_click_action = self.attack_action
        click_configs = [cc]

        self.attacks_label = LabelWidget(ClientWindow.Instance.font, '0', bg_color=LRED, click_configs=click_configs)
        self.add_widget(self.attacks_label)

    def load(self, data):
        attacks_text = ''
        card = None
        self.last_attacks_left = 0

        if data is not None:
            attacks_text = f' {data.attacksLeft}'
            self.last_attacks_left = data.attacksLeft
            card = data.card

        self.card.load(card)
        self.attacks_label.set_text(attacks_text)

    def attack_action(self):
        ClientWindow.Instance.send_response(f'attack {self.lane_i}')


class LanesContainer(VerContainer):
    def __init__(self, parent: 'PlayerContainer', outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.pc_parent = parent
        self.units: list[UnitContainer] = []

        # TODO should be done with match config
        # self.set_up_lanes(3)

    def set_up_lanes(self, lane_count: int):
        self.lanes_container = HorContainer()
        # sep = RectWidget(BLACK, max_width=1)
        sep = SPACE_FILLER
        self.lanes_container.add_widget(sep)
        for i in range(lane_count):
            u = UnitContainer(i)
            self.units += [u]
            self.lanes_container.add_widget(u)
            self.lanes_container.add_widget(sep)
        # self.lanes_container.add_widget(SPACE_FILLER)
        self.add_widget(SPACE_FILLER)
        self.add_widget(self.lanes_container)
        self.add_widget(SPACE_FILLER)

    def load(self, data):
        for i, unit in enumerate(self.units):
            unit.load(data[i])


class TreasuresContainer(StackContainer):
    def __init__(self, parent: 'PlayerContainer', outline_color: tuple[int, int, int] = None):
        # TODO configure optimal max_per_line
        super().__init__(3, outline_color)

    def load(self, data):
        self.widgets = [RectWidget()]
        self.last_container = None
        for card in data:
            w = TreasureWidget()
            w.load(card)
            self.add_widget(w)
        # return super().load(data)


class PlayerContainer(HorContainer):
    def __init__(self):
        super().__init__(RED)

        self.info_c = InfoContainer(self)
        self.add_widget(self.info_c)

        self.lanes_c = LanesContainer(self)
        self.add_widget(self.lanes_c)

        self.treasures_c = TreasuresContainer(self)
        self.add_widget(self.treasures_c)

    def load(self, data):
        self.info_c.load(data)
        self.lanes_c.load(data.units)
        self.treasures_c.load(data.treasures)


BETWEEN_CARDS = 2
class HandContainer(HorContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.start_widget = RectWidget(max_width=0, min_height=4+CARD_HEIGHT, max_height=4+CARD_HEIGHT)
        self.end_widget = SPACE_FILLER

        self.set_widgets([])

        self.last_state = None

    def set_widgets(self, widgets: list[Widget]):
        self.widgets.clear()
        self.widgets += [self.start_widget]
        self.widgets += widgets
        self.widgets += [self.end_widget]

    def load(self, data):
        w = []
        for card in data:
            w += [RectWidget(WHITE, max_width=BETWEEN_CARDS)]
            w += [CardInHandWidget.from_card(card)]
        self.set_widgets(w)


HOST = 'localhost'
PORT = 8080
class ClientWindow(Window):
    def __init__(self):
        ClientWindow.Instance = self

        super().__init__()

        self.init_ui()
        self.set_title('client test')

        # connection

        self.last_state = None

    def config_connection(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((HOST, PORT))
        self.sconfig = parse_state(self.read_msg())
        self.top_player.lanes_c.set_up_lanes(self.sconfig.lane_count)
        self.bottom_player.lanes_c.set_up_lanes(self.sconfig.lane_count)
        # TODO untilize match configuration

        self.sock.settimeout(.01)


    def init_ui(self):
        # TODO remove, for easier debugging
        self.font = None
        try:
            ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 14)
        except:
            ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 14)

        all_container = HorContainer()
        self.container = all_container

        # set up player data
        self.init_player_data_ui()

        # set up all other data data
        self.init_other_data_ui()

    def init_player_data_ui(self):
        container = VerContainer()
        self.top_player = PlayerContainer()
        self.bottom_player = PlayerContainer()
        self.hand = HandContainer()

        self.container.add_widget(container)

        mid_container = HorContainer()
        self.mid_label = LabelWidget(self.font, ' ')
        mid_container.add_widget(self.mid_label)
        mid_container.add_widget(SPACE_FILLER)

        sep = RectWidget(WHITE, max_height=10)
        container.add_widget(self.top_player)
        container.add_widget(sep)
        container.add_widget(self.bottom_player)
        container.add_widget(mid_container)
        container.add_widget(self.hand)
        container.add_widget(sep)

    def init_other_data_ui(self):
        container = VerContainer()
        container.add_widget(SPACE_FILLER)
        
        self.last_played_card_container = HorContainer()
        self.last_played_card = CardWidget([])
        self.last_played_card_container.add_widget(SPACE_FILLER)
        self.last_played_card_container.add_widget(self.last_played_card)
        self.last_played_card_container.add_widget(SPACE_FILLER)
        container.add_widget(LabelWidget(self.font, 'Last played:'))
        self.last_played_label = LabelWidget(self.font, '')
        container.add_widget(self.last_played_card_container)
        container.add_widget(self.last_played_label)
        container.add_widget(SPACE_FILLER)
        self.logs_container = LogsContainer()
        container.add_widget(self.logs_container)
        container.add_widget(SPACE_FILLER)

        self.container.add_widget(container)

    def draw(self):
        if self.last_state is None:
            return
        
        self.coord_dict = {}
        super().draw()
        if self.last_state is not None and not self.last_state.sourceID in self.coord_dict:
            return
        
        coord = self.coord_dict[self.last_state.sourceID]
        util.draw_arrow(self.screen, (coord[0] + CARD_WIDTH/2, coord[1] + CARD_HEIGHT/2), pg.mouse.get_pos(), 3)

    # fuze = 0
    def update(self):
        super().update()
        statej = self.read_msg()
        if statej != '':
            parsed = parse_state(statej)
            self.load(parsed)
        # if self.fuze == 1:
        #     return
        # state = test_state()
        # self.load(state)
        # self.fuze = 1

    def load(self, state):
        self.last_state = state
        # self.top_player.load(state.players[1-state.myData.playerI])
        # self.bottom_player.load(state.players[state.myData.playerI])
        self.top_player.load(state.players[1])
        self.bottom_player.load(state.players[0])
        self.hand.load(state.myData.hand)

        # print(state.newLogs)

        self.mid_label.set_text(f'({state.request}) {state.prompt}')

        if state.lastPlayed is not None:
            self.last_played_card.load(state.lastPlayed.card)
            self.last_played_label.set_text(f'(player: {state.lastPlayed.playerName})')

        self.logs_container.load(state.newLogs)
  
    def send_response(self, response: str):
        self.send_msg(response)

    def read_msg(self):
        message = ''
        try :
            message_length_bytes = self.sock.recv(4)
            # print(message_length_bytes)
            message_length = int.from_bytes(message_length_bytes, byteorder='little')
            # print(f'Message length: {message_length}')

            # Receive the message itself
            while len(message) < message_length:
                message_bytes = self.sock.recv(message_length)
                message += message_bytes.decode('utf-8')

            # print('Read: ' + message)
        except socket.timeout:
            message = ''

        return message
    
    def send_msg(self, msg: str):
        message_length = len(msg)
        message_length_bytes = message_length.to_bytes(4, byteorder='little')
        message_bytes = msg.encode('utf-8')
        message_with_length = message_length_bytes + message_bytes
        self.sock.sendall(message_with_length)

    def process_key(self, event: pg.event.Event):
        super().process_key(event)
        if event.key == pg.K_SPACE:
            self.send_response('pass')
            return