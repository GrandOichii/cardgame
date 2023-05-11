import pygame as pg

import front.test_py.util as util
from front.test_py.frame import *
# from front.test_py.frame import Rect, WindowConfigs

CARD_SIZE_RATIO = 7 / 5
CARD_WIDTH = 110
CARD_HEIGHT = CARD_SIZE_RATIO * CARD_WIDTH

CARD_NAME_OFFSET = 18


class CardWidget(RectWidget):
    def from_card(card):
        result = CardWidget()
        result.load(card)
        return result

    def __init__(self):
        super().__init__(LGRAY, CARD_WIDTH, CARD_HEIGHT, CARD_WIDTH, CARD_HEIGHT, CARD_WIDTH, CARD_HEIGHT)
        
        self.image = pg.Surface((CARD_WIDTH, CARD_HEIGHT))
        self.image.fill(WHITE)


    def load(self, data):
        if data is None:
            self.image.fill(WHITE)
            return
        
        self.image.fill(BLACK)
        pg.draw.rect(self.image, self.bg_color, (1, 1, CARD_WIDTH-2, CARD_NAME_OFFSET))

        pg.draw.rect(self.image, self.bg_color, (1, 2 + CARD_NAME_OFFSET, CARD_WIDTH-2, CARD_NAME_OFFSET))

        pg.draw.rect(self.image, self.bg_color, (1, 3 + CARD_NAME_OFFSET*2, CARD_WIDTH-2, CARD_HEIGHT-4-2*CARD_NAME_OFFSET))
        
        from front.test_py.client import ClientWindow
        font = ClientWindow.Instance.font

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
        surface.blit(self.image, bounds.to_tuple())
        return bounds.width, bounds.height
        # return super().draw(surface, bounds, configs)