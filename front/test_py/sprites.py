
import pygame as pg
import colors
import util
import client

# from main import *

CARD_RATIO = 25 / 35
CARD_HEIGHT = 150
CARD_WIDTH = CARD_HEIGHT * CARD_RATIO

CARD_NAME_OFFSET = 18


def outline_rect(width: int, height: int, color, outline_color=colors.BLACK) -> pg.Surface:
    result = pg.Surface((width, height))
    result.fill(outline_color)        
    pg.draw.rect(result, color, (1, 1, width-2, height-2))
    return result



class BoxSprite(pg.sprite.Sprite):
    def __init__(self, width: int, height: int) -> None:
        super().__init__()
        # sprites = pg.sprite.Group()

        self.image = outline_rect(width, height, colors.WHITE)
        self.rect = self.image.get_rect()


INFO_WIDTH = CARD_WIDTH + 20


class Board:
    def __init__(self):
        self.sprites = pg.sprite.Group()

        self.boards: list[Board] = []

        self.state = None

        self.x, self.y = 0, 0


    def set_pos(self, pos: tuple[int, int]):
        self.x, self.y = pos
        for b in self.boards:
            b.set_pos(pos)

        for sprite in self.sprites:
            r = sprite.rect
            r.x += pos[0]
            r.y += pos[1]


    def draw(self, surface: pg.Surface):
        self.sprites.draw(surface)
        for board in self.boards:
            board.draw(surface)


    def load(self, state):
        self.state = state


class InfoBoard(Board):

    def __init__(self, width: int, height: int):
        super().__init__()

        self.width = width
        self.height = height

        self.lines = [
            [('s:name', colors.BLACK)],
            [(' ', colors.BLACK)],
            [('Life: ', colors.BLACK), ('s:life', colors.RED)],
            [('Energy: ', colors.BLACK), ('s:energy', colors.BLUE)],
            [('Hand: ', colors.BLACK), ('s:handCount', colors.GRAY)],
            [('Deck: ', colors.BLACK), ('s:deckCount', colors.GRAY)],
        ]
        # "name":"","handCount":5,"deckCount":27,
        
        self.font = pg.font.SysFont(None, 24)

        self.bg = BoxSprite(width, height)
        self.bond = None

        self.load(None)


    def load(self, state):
        # state is player state, extract player info
        self.sprites.empty()

        self.sprites.add(self.bg)
        # TODO update bond

        if state is None: return
        self.bond = CardSprite(state.bond)
        self.bond.rect.y = self.y + self.height - self.bond.rect.height - 5
        self.bond.rect.centerx = self.width / 2
        self.sprites.add(self.bond)

        return super().load(state)
    

    def draw(self, surface: pg.Surface):
        super().draw(surface)

        if not self.state: return

        y = self.y + 2
        x = self.x + 2
        for line in self.lines:
            cx = x
            lh = 0
            for token in line:
                s = token[0]
                color = token[1]
                if s.startswith('s:'):
                    s = s[2:]
                    s = self.state.__dict__[s]
                img = self.font.render(str(s), True, color)
                lh = img.get_height()
                surface.blit(img, (cx, y))
                cx += img.get_width()
            y += lh


class PlayerBoard(Board):
    def __init__(self, wwidth: int, height: int):
        super().__init__()

        bg = BoxSprite(wwidth, height)
        self.sprites.add(bg)

        self.info = InfoBoard(INFO_WIDTH, height)
        self.boards += [self.info]


    def load(self, state):
        self.info.load(state)
        return super().load(state)


SELECT_COLOR = colors.GREEN


class SelectableCardSpriteGroup(pg.sprite.Group):

    def draw(self, surface):
        mpos = pg.mouse.get_pos()
        sprites = self.sprites()
        surface_blit = surface.blit
        for spr in sprites:

            if spr.rect.collidepoint(mpos):
                pg.draw.rect(surface, colors.GREEN, (spr.rect.x-1, spr.rect.y-1, spr.rect.width+2, spr.rect.height+2))
                if client.Client.INSTANCE.clicked:
                    msg = f'play {spr.card.id}'
                    print(msg)
                    client.Client.INSTANCE.send_msg(msg)

            self.spritedict[spr] = surface_blit(spr.image, spr.rect)
            # pg.draw.rect(surface, colors.WHITE, (spr.rect.topleft, (0, 0)))

        self.lostsprites = []


BETWEEN_CARDS = 10

class HandBoard(Board):
    def __init__(self, width: int, height: int):
        super().__init__()

        self.sprites = SelectableCardSpriteGroup()
        self.height = height


    def load(self, state):
        super().load(state)

        self.sprites.empty()
        x = self.x + 1
        for card in state:
            s = CardSprite(card)
            s.rect.centery = self.height / 2
            s.rect.y += self.y
            s.rect.x = x

            x += s.rect.width + BETWEEN_CARDS
            self.sprites.add(s)


CARD_FONT_SIZE = 12


class CardSprite(pg.sprite.Sprite):
    FONT: pg.font.Font = None

    def __init__(self, card_state):
        if not CardSprite.FONT:
            CardSprite.FONT = pg.font.Font('fonts/Montserrat-Thin.ttf', 12)

        super().__init__()
        self.card = card_state

        self.image = pg.Surface((CARD_WIDTH, CARD_HEIGHT))
        self.contruct_image(card_state)
        # self.image.fill(self.image)
        self.rect = self.image.get_rect()
        # self.rect.center = (800 / 2, 600 / 2)


    def contruct_image(self, card_state):
        bg_color = colors.LGRAY

        self.image.fill(colors.BLACK)        
        pg.draw.rect(self.image, bg_color, (1, 1, CARD_WIDTH-2, CARD_NAME_OFFSET))

        pg.draw.rect(self.image, bg_color, (1, 2 + CARD_NAME_OFFSET, CARD_WIDTH-2, CARD_NAME_OFFSET))

        pg.draw.rect(self.image, bg_color, (1, 3 + CARD_NAME_OFFSET*2, CARD_WIDTH-2, CARD_HEIGHT-4-2*CARD_NAME_OFFSET))


        img = CardSprite.FONT.render(card_state.name, False, colors.GRAY)
        r = img.get_rect()
        r.centery = CARD_NAME_OFFSET / 2
        self.image.blit(img, (r.x+1, r.y+1))

        img = CardSprite.FONT.render(card_state.type, False, colors.GRAY)
        r = img.get_rect()
        r.centery = (CARD_NAME_OFFSET+1)*3 / 2
        self.image.blit(img, (r.x+1, r.y+1))

        if self.card.type not in ['Bond', 'Source']:
            img = CardSprite.FONT.render(str(card_state.cost).center(4), False, colors.BLACK, colors.LBLUE)
            r = img.get_rect()
            r.y = 1
            r.x = CARD_WIDTH - r.width - 1
            self.image.blit(img, (r.x, r.y))

        # self.image.blit(img, (r.x+1, r.y+1))

        if self.card.type == 'Unit':
            img = CardSprite.FONT.render(str(card_state.power).center(4), False, colors.BLACK, colors.LRED)
            r = img.get_rect()
            r.y = CARD_HEIGHT - r.height - 1
            r.x = 1
            self.image.blit(img, (r.x, r.y))

        if self.card.type in ['Unit', 'Treasure']:
            img = CardSprite.FONT.render(str(card_state.life).center(4), False, colors.BLACK, colors.LGREEN)
            r = img.get_rect()
            r.y = CARD_HEIGHT - r.height - 1
            r.x = CARD_WIDTH - r.width - 1
            self.image.blit(img, (r.x, r.y))

        util.blit_text(self.image, card_state.text, (1, CARD_NAME_OFFSET*2 + 3), CardSprite.FONT, colors.BLACK)