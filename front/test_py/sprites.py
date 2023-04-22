
import pygame as pg
import colors
import util
import client

# from main import *

CARD_RATIO = 25 / 35
CARD_HEIGHT = 150
CARD_WIDTH = CARD_HEIGHT * CARD_RATIO

CARD_NAME_OFFSET = 18

TREASURE_ZONE_CARD_COUNT_X = 3
BETWEEN_TREASURES = 2


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
    def __init__(self, width, height):
        self.width = width
        self.height = height

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
        super().__init__(width, height)

        self.lines = [
            [('s:name', colors.BLACK)],
            [(' ', colors.BLACK)],
            [('Life: ', colors.BLACK), ('s:life', colors.RED)],
            [('Energy: ', colors.BLACK), ('s:energy', colors.BLUE), ('/', colors.BLACK), ('s:maxEnergy', colors.BLUE)],
            [('Hand: ', colors.BLACK), ('s:handCount', colors.GRAY)],
            [('Deck: ', colors.BLACK), ('s:deckCount', colors.GRAY)],
        ]
        
        self.font = pg.font.SysFont(None, 24)

        self.bg = BoxSprite(width, height)
        self.bond = None

        self.bond_group = ClickableSpriteGroup([
            ClickConfig(
                lambda element: client.Client.INSTANCE.last_state.request == 'in_play' and element.card.id in client.Client.INSTANCE.last_state.args,
                lambda element: f'{element.card.id}'
            )
        ])

        self.load(None)


    def load(self, state):
        # state is player state, extract player info
        self.sprites.empty()
        self.bond_group.empty()

        self.sprites.add(self.bg)

        if state is None: return
        self.bond = CardSprite(state.bond)
        self.bond.rect.y = self.y + self.height - self.bond.rect.height - 5
        self.bond.rect.centerx = self.width / 2
        self.bond_group.add(self.bond)

        return super().load(state)
    

    def draw(self, surface: pg.Surface):
        super().draw(surface)
        self.bond_group.draw(surface)

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


CHARM_HEIGHT = 40


class PlayerBoard(Board):
    def __init__(self, width: int, height: int):
        super().__init__(width, height)

        bg = BoxSprite(width, height)
        self.sprites.add(bg)

        self.info = InfoBoard(INFO_WIDTH, height)
        self.boards += [self.info]

        self.lanes = ClickableBoardGroup([
            ClickConfig (
                lambda _: client.Client.INSTANCE.last_state.request == 'pick lane',
                lambda element: f'{element.lane_i}'
            ),
            ClickConfig (
                lambda element: element.unit is not None and element.unit.attacksLeft > 0 and client.Client.INSTANCE.last_state.request == 'enter command',
                # TODO make more complex
                lambda element: f'attack {element.lane_i}'
            ),
            ClickConfig (
                lambda element: element.unit is not None and client.Client.INSTANCE.last_state.request == 'in_play' and element.unit.card.id in client.Client.INSTANCE.last_state.args,
                lambda element: f'{element.unit.card.id}'
            )
        ])

        # TODO treasure zone

    def player_draw(self, surface: pg.Surface, current: bool):
        super().draw(surface)
        self.lanes.draw(surface)

        if not current: return
        pg.draw.rect(surface, colors.RED, (self.x, self.y, 10, 10))

    def load(self, state):
        self.info.load(state)
        for lane in self.lanes:
            lane.load(state)
        self.treasure_board.load(state.treasures)
        return super().load(state)

    def create_lanes(self, lane_count: int):
        # hch = CHARM_HEIGHT // 2
        treasures_width = TREASURE_ZONE_CARD_COUNT_X * (CARD_WIDTH + BETWEEN_TREASURES) + 1
        lane_width = (self.width - INFO_WIDTH - treasures_width) / lane_count
        x = self.x + INFO_WIDTH
        y = self.y
        for i in range(lane_count):
            lane = LaneBoard(lane_width - 2, self.height - 5, i)
            lane.set_pos((x + 1, y + 2))
            x += lane_width
            # self.boards += [lane]

            self.lanes.add(lane)
        
        # treasure board
        self.treasure_board = TreasureBoard(treasures_width, self.height)
        self.treasure_board.set_pos((x, y))
        self.boards += [self.treasure_board]

    def draw(self, surface: pg.Surface):
        super().draw(surface)


SELECT_COLOR = colors.GREEN


class ClickConfig:
    def __init__(self, checker, prefix_constructor) -> None:
        self.checker = checker
        self.prefix_constructor = prefix_constructor


class ClickableGroup:

    def __init__(self, configs: list[ClickConfig]):
        super().__init__()

        self.configs = configs
        self.elements = []

    def add(self, el):
        self.elements += [el]

    def __getitem__(self, key):
        return self.elements[key]

    def draw_el(self, surface, el):
        pass

    def get_rect(self, el):
        pass

    def draw(self, surface):
        mpos = pg.mouse.get_pos()
        # surface_blit = surface.blit
        for sprite in self.elements:

            rect = self.get_rect(sprite)

            # print(rect)
            clicked = client.Client.INSTANCE.clicked
            if rect.collidepoint(mpos):
                for config in self.configs:
                    if not config.checker(sprite): continue
                    pg.draw.rect(surface, colors.GREEN, (rect.x-1, rect.y-1, rect.width+2, rect.height+2))
                    if not clicked: continue
                    msg = config.prefix_constructor(sprite)
                    client.Client.INSTANCE.send_msg(msg)
                    break
            self.draw_el(surface, sprite)

            # pg.draw.rect(surface, colors.RED, rect)

        self.lostsprites = []

    def empty(self):
        self.elements = []


class ClickableSpriteGroup(ClickableGroup):

    def draw_el(self, surface, el):
        surface.blit(el.image, el.rect)

    def get_rect(self, el):
        return el.rect


class ClickableBoardGroup(ClickableGroup):

    def draw_el(self, surface, el):
        el.draw(surface)

    def get_rect(self, el):
        return pg.Rect(el.x, el.y, el.width, el.height)


BETWEEN_CARDS = 10


class HandBoard(Board):
    def __init__(self, width: int, height: int):
        super().__init__(width, height)
        
        self.sprites = ClickableSpriteGroup([
            ClickConfig(
                lambda element: client.Client.INSTANCE.last_state.request == 'enter command',
                lambda element: f'play {element.card.id}'
            ),
            ClickConfig(
                lambda element: client.Client.INSTANCE.last_state.request == 'in_hand' and element.card.id in client.Client.INSTANCE.last_state.args,
                lambda element: f'{element.card.id}'
            )
        ])

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
    TEXT_FONT: pg.font.Font = None

    def __init__(self, card_state):
        if not CardSprite.FONT:
            CardSprite.FONT = pg.font.Font('fonts/Montserrat-Thin.ttf', 12)
            CardSprite.TEXT_FONT = pg.font.Font('fonts/Montserrat-Thin.ttf', 10)
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

        text = str(card_state.text)
        # print(card_state.mutable)
        m = card_state.mutable.__dict__
        for pair in m.items():
            key = pair[0]
            value = pair[1]
            text = card_state.text.replace(f'[{key}]', str(value.current))

        util.blit_text(self.image, text, (1, CARD_NAME_OFFSET*2 + 3), CardSprite.TEXT_FONT, colors.BLACK)


class UnitCardSprite(CardSprite):
    def __init__(self, unit):
        super().__init__(unit.card)
        self.unit = unit


class LaneBoard(Board):
    def __init__(self, width: int, height: int, lane_i: int):
        super().__init__(width, height)

        self.unit = None
        self.lane_i = lane_i

        self.bg = BoxSprite(width, height)
        self.sprites.add(self.bg)


    def load(self, state):
        super().load(state)
        self.sprites.empty()

        self.sprites.add(self.bg)

        self.unit = state.units[self.lane_i]
        if self.unit is None: return

        # print(self.unit)
        c = UnitCardSprite(self.unit)
        c.rect.x = self.x + (self.width - c.rect.width) // 2
        c.rect.y = self.y + (self.height - c.rect.height) // 2

        self.sprites.add(c)


class TreasureBoard(Board):
    def __init__(self, width, height):
        super().__init__(width, height)
        # TODO

        self.bg = BoxSprite(width, height)
        self.sprites.add(self.bg)

        self.cards = ClickableSpriteGroup([
            ClickConfig(
                lambda element: element.card is not None and client.Client.INSTANCE.last_state.request == 'in_play' and element.card.id in client.Client.INSTANCE.last_state.args,
                lambda element: f'{element.card.id}'
            )
        ])
        # self.boards += [self.cards]

    def load(self, state):
        super().load(state)

        # self.sprites.empty()
        # self.sprites.add(self.bg)

        x = self.x
        y = self.y
        count = 0
        self.cards.empty()
        for card in state:
            card = CardSprite(card)
            card.rect.x = x
            card.rect.y = y
            x += card.rect.width + BETWEEN_TREASURES
            self.cards.add(card)
            count += 1
            if count == TREASURE_ZONE_CARD_COUNT_X:
                count = 0
                x = self.x
                y += card.rect.height + BETWEEN_TREASURES

    def draw(self, surface: pg.Surface):
        super().draw(surface)
        self.cards.draw(surface)            
        
# class LanesBoard(Board):
#     def __init__(self, lane_count: int, width: int, height: int):
#         super().__init__()

#         self.lane_count = lane_count

#         bg = BoxSprite(width, height)
#         self.sprites.add(bg)


class MatchResultSprite(pg.sprite.Sprite):
    def __init__(self, won: bool, wWidth: int, wHeight: int):
        super().__init__()
        
        font = pg.font.Font('fonts/Montserrat-Thin.ttf', 48)
        font.set_bold(True)
        self.image = None
        if won:
            self.image = font.render('Won!', False, colors.GREEN)
        else:
            self.image = font.render('Lost!', False, colors.RED)
        self.rect = self.image.get_rect()
        self.rect.center = (wWidth / 2, wHeight / 2)


class PromptMessageSprite(pg.sprite.Sprite):
    def __init__(self, message: str):
        super().__init__()

        self.font = pg.font.Font('fonts/Montserrat-Thin.ttf', 18)

        self.load(message)

    def load(self, message: str):
        self.image = self.font.render(message, False, colors.BLACK)
        self.rect = self.image.get_rect()