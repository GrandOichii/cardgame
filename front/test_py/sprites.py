import pygame as pg
from front.test_py.frame import BLACK, WHITE, ClickConfig, Rect, WindowConfigs, pg

import front.test_py.util as util
from front.test_py.frame import *
# from front.test_py.client import ClientWindow
import front.test_py.client as client
# from front.test_py.frame import Rect, WindowConfigs

CARD_SIZE_RATIO = 7 / 5
CARD_WIDTH = 130
CARD_HEIGHT = CARD_SIZE_RATIO * CARD_WIDTH

CARD_NAME_OFFSET = 18


class CardWidget(RectWidget):
    # def from_card(card):
    #     result = CardWidget()
    #     result.load(card)
    #     return result

    def __init__(self, parent_window: 'client.ClientWindow', click_configs: list[ClickConfig]):
        self.last_data = None
        self.g_window = parent_window
        RectWidget.__init__(self, LGRAY, CARD_WIDTH, CARD_HEIGHT, CARD_WIDTH, CARD_HEIGHT, CARD_WIDTH, CARD_HEIGHT, click_configs)
        
        self.image = pg.Surface((CARD_WIDTH, CARD_HEIGHT))
        self.image.fill(WHITE)

    def load(self, data):
        if data is None:
            self.image.fill(WHITE)
            return
        self.last_data = data
        
        self.image.fill(BLACK)
        pg.draw.rect(self.image, self.bg_color, (1, 1, CARD_WIDTH-2, CARD_NAME_OFFSET))

        pg.draw.rect(self.image, self.bg_color, (1, 2 + CARD_NAME_OFFSET, CARD_WIDTH-2, CARD_NAME_OFFSET))

        pg.draw.rect(self.image, self.bg_color, (1, 3 + CARD_NAME_OFFSET*2, CARD_WIDTH-2, CARD_HEIGHT-4-2*CARD_NAME_OFFSET))
        
        font = self.g_window.font

        img = font.render(data.name, False, GRAY)
        r = img.get_rect()
        r.centery = CARD_NAME_OFFSET / 2
        self.image.blit(img, (r.x+1, r.y+1))

        img = font.render(data.type, False, GRAY)
        r = img.get_rect()
        r.centery = (CARD_NAME_OFFSET+1)*3 / 2
        self.image.blit(img, (r.x+1, r.y+1))

        if data.type not in ['Bond', 'Source']:
            img = font.render(str(data.cost).center(4), False, BLACK, LBLUE)
            r = img.get_rect()
            r.y = 1
            r.x = CARD_WIDTH - r.width - 1
            self.image.blit(img, (r.x, r.y))

        # self.image.blit(img, (r.x+1, r.y+1))

        if data.type == 'Unit':
            img = font.render(str(data.power).center(4), False, BLACK, LRED)
            r = img.get_rect()
            r.y = CARD_HEIGHT - r.height - 1
            r.x = 1
            self.image.blit(img, (r.x, r.y))

        if data.type in ['Unit', 'Treasure']:
            img = font.render(str(data.life).center(4), False, BLACK, LGREEN)
            r = img.get_rect()
            r.y = CARD_HEIGHT - r.height - 1
            r.x = CARD_WIDTH - r.width - 1
            self.image.blit(img, (r.x, r.y))

        text = str(data.text)
        # print(data.mutable)
        m = data.mutable.__dict__
        for pair in m.items():
            key = pair[0]
            value = pair[1]
            text = text.replace(f'[{key}]', str(value.current))
        text = text.replace('[CARDNAME]', data.name)

        util.blit_text(self.image, text, (1, CARD_NAME_OFFSET*2 + 3), font, BLACK)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        super().draw(surface, bounds, configs)
        surface.blit(self.image, bounds.to_tuple())
        return bounds.width, bounds.height
        # return super().draw(surface, bounds, configs)


class CardInHandWidget(CardWidget):
    def from_card(parent_window: 'client.ClientWindow', card):
        result = CardInHandWidget(parent_window)
        result.load(card)
        return result
    
    def __init__(self, parent_window: 'client.ClientWindow'):
        cc1 = ClickConfig()
        cc1.mouse_over_condition = lambda: self.g_window.last_state.request == 'enter command'
        cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc1.mouse_click_condition = lambda: True
        cc1.mouse_click_action = self.play_action

        cc2 = ClickConfig()
        cc2.mouse_over_condition = lambda: self.g_window.last_state.request == 'in_hand' and self.last_data.id in self.g_window.last_state.args
        cc2.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc2.mouse_click_condition = lambda: True
        cc2.mouse_click_action = self.in_hand_action

        click_configs = [
            cc1,
            cc2
        ]
    
        super().__init__(parent_window, click_configs)

    def play_action(self):
        self.g_window.send_response(f'play {self.last_data.id}')

    def in_hand_action(self):
        self.g_window.send_response(f'{self.last_data.id}')

    
class CardInPlayWidget(CardWidget):
    def __init__(self, parent_window: 'client.ClientWindow'):

        cc1 = ClickConfig()
        cc1.mouse_over_condition = lambda: self.last_data is not None and self.g_window.last_state.request == 'target' and self.last_data.id in self.g_window.last_state.args
        cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc1.mouse_click_condition = lambda: True
        cc1.mouse_click_action = self.target_action

        click_configs = [cc1]

        super().__init__(parent_window, click_configs)

    def target_action(self):
        self.g_window.send_response(f'{self.last_data.id}')

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        if self.last_data is not None:
            self.g_window.coord_dict[self.last_data.id] = (bounds.x, bounds.y)

        return super().draw(surface, bounds, configs)


class TreasureWidget(CardInPlayWidget):
    def __init__(self, parent_window: 'client.ClientWindow'):
        super().__init__(parent_window)

        # cc1 = ClickConfig()
        # cc1.mouse_over_condition = lambda: self.g_window.last_state.request == 'enter command'
        # cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        # cc1.mouse_click_condition = lambda: True
        # cc1.mouse_click_action = self.attack_action

        cc1 = ClickConfig()
        cc1.mouse_over_condition = lambda: self.g_window.last_state.request == 'pick attack target'
        cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, BLUE, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc1.mouse_click_condition = lambda: True
        cc1.mouse_click_action = self.pick_attack_action

        # self.click_configs += [cc1]
        self.click_configs += [cc1]

    def pick_attack_action(self):
        self.g_window.send_response(f'{self.last_data.id}')


class UnitCardWidget(CardInPlayWidget):
    def __init__(self, parent_window: 'client.ClientWindow', lane_i: int):
        super().__init__(parent_window)

        self.lane_i = lane_i

        # cc1 = ClickConfig()
        # cc1.mouse_over_condition = lambda: self.g_window.last_state.request == 'enter command'
        # cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        # cc1.mouse_click_condition = lambda: True
        # cc1.mouse_click_action = self.attack_action

        cc1 = ClickConfig()
        cc1.mouse_over_condition = lambda: self.g_window.last_state.request == 'pick lane'
        cc1.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, BLUE, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc1.mouse_click_condition = lambda: True
        cc1.mouse_click_action = self.pick_lane_action

        # self.click_configs += [cc1]
        self.click_configs += [cc1]

    # def attack_action(self):
    #     client.ClientWindow.send_response(f'attack {self.lane_i}')

    def pick_lane_action(self):
        self.g_window.send_response(f'{self.lane_i}')