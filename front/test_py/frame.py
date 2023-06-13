# import client

# client.Client.INSTANCE = client.Client()
# client.Client.INSTANCE.start()


import os
import random
# random.seed(0)
import pygame as pg
# from front.test_py.frame import WHITE, ClickConfig
# from front.test_py.frame import ClickConfig, Rect, WindowConfigs
# from front.test_py.frame import Widget
# from front.test_py.frame import Rect, WindowConfigs


from front.test_py.sizeconfig import size_config
# from sizeconfig import size_config


BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
RED = (255, 0, 0)
LRED = (255, 100, 100)
GREEN = (0, 255, 0)
LGREEN = (100, 255, 100)
BLUE = (0, 0, 255)
LBLUE = (100, 100, 255)
GRAY = (80, 80, 80)
LGRAY = (200, 200, 200)


def random_color():
    return (random.randint(0, 255), random.randint(0, 255), random.randint(0, 255))


class WindowConfigs:
    def __init__(self) -> None:
        self.wireframe = False
        self.mouse_clicked = False

    def toggle_wireframe(self):
        self.wireframe = not self.wireframe


class Window:
    # def __new__(cls):
    #     if not hasattr(cls, 'instance'):
    #         cls.instance = super(Window, cls).__new__(cls)
    #     return cls.instance
    
    def __init__(self):
        pg.init()
        self.configs = WindowConfigs()


        self.key_map = {
            pg.K_1: self.configs.toggle_wireframe
        }
        self.fps = 60
        self.set_screen_size(1000, 1000 * 6 / 8)

        self.clock = pg.time.Clock()

        self.running = True

        self.container = Container()

    def set_title(self, title: str):
        pg.display.set_caption(title)

    def go_fullscreen(self):
        if self.fullscreen:
            return
        self.fullscreen = True
        self.screen = pg.display.set_mode((0, 0), pg.FULLSCREEN)
        self.width, self.height = self.screen.get_size()

    def set_screen_size(self, width: int, height: int):
        self.fullscreen = False
        self.screen = pg.display.set_mode((width, height))

        self.width, self.height = self.screen.get_size()

    def run(self):
        while self.running:
            # clock
            self.clock.tick(self.fps)

            self.update()

            # events
            self.events()

            # draw
            self.draw()

            # refresh screen
            pg.display.flip()
        self.on_close()

    def on_close(self):
        pg.quit()

    def update(self):
        pass

    def draw(self):
        self.screen.fill(WHITE)
        self.container._draw(self.screen, Rect(0, 0, self.width, self.height), self.configs)
        self.configs.mouse_clicked = False

    def events(self):
        for event in pg.event.get():
            if event.type == pg.QUIT:
                self.running = False
                return
            if event.type == pg.KEYDOWN:
                self.process_key(event)
            if event.type == pg.MOUSEBUTTONDOWN:
                self.configs.mouse_clicked = True

    def process_key(self, event: pg.event.Event):
        key = event.key
        if not key in self.key_map: return

        self.key_map[key]()


class FontContainer:
    def __init__(self, fpath: str):
        self.fpath = fpath
        self.size_index = {}

    def get(self, size: int):
        if not size in self.size_index:
            font = pg.font.Font(self.fpath, size)
            self.size_index[size] = font
        return self.size_index[size]


class ContentPool:
    def __init__(self):
        self.findex: dict[str, FontContainer] = {}

    def load_font(self, font_key: str, path: str):
        if path in self.findex:
            return
        self.findex[font_key] = FontContainer(path)

    def get_font(self, font_key: str, size: int):
        if not font_key in self.findex:
            raise Exception(f'Font key {font_key} is not present in ContentLoader')
        return self.findex[font_key].get(size)
ContentPool.Instance = ContentPool()
        

def between(min: int, cur: int, max: int) -> int:
    if cur < min:
        return min
    if max == -1:
        return cur
    # if cur > max:
    #     return max
    return max


class Rect:
    def __init__(self, x=0, y=0, width=0, height=0,) -> None:
        self.y = y
        self.x = x
        self.width = width
        self.height = height

    def from_tuple(t: tuple[int, int, int, int]):
        return Rect(t[0], t[1], t[2], t[3])
    
    def copy(self) -> 'Rect':
        return Rect(self.x, self.y, self.width, self.height)
    
    def __str__(self) -> str:
        return f'x:{self.x} y:{self.y} w:{self.width} h:{self.height}'
    
    def with_mod(self, xmod: int=0, ymod: int=0, widthmod: int=0, heightmod: int=0):
        return Rect(self.x + xmod, self.y + ymod, self.width + widthmod, self.height + heightmod)

    def to_tuple(self) -> tuple[int, int, int, int]:
        return (self.x, self.y, self.width, self.height)


class ClickConfig:
    def __init__(self):
        self.mouse_over_condition = None
        self.mouse_over_action = None
        
        self.mouse_click_condition = None
        self.mouse_click_action = None


class Widget:
    def __init__(self, pref_width: int=50, pref_height: int=50, min_width: int=0, min_height: int=0, max_width: int=-1, max_height: int=-1, click_configs: list[ClickConfig]=None):
        self.parent: Container = None
        self.x, self.y = 0, 0
        self.min_width, self.max_width = min_width, max_width
        self.min_height, self.max_height = min_height, max_height
        
        self.set_pref_width(pref_width)
        self.set_pref_height(pref_height)

        self.click_configs = click_configs
        if click_configs is None:
            self.click_configs = []

    def get_min_width(self):
        return self.min_width
    
    def get_min_height(self):
        return self.min_height
    
    def get_max_width(self):
        return self.max_width
    
    def get_max_height(self):
        return self.max_height

    def set_pref_width(self, value: str):
        self.pref_width = max(value, self.min_width)
        if self.max_width == -1:
            return
        self.pref_width = min(self.pref_width, self.max_width)

    def set_pref_height(self, value: str):
        self.pref_height = max(value, self.min_height)
        if self.max_height == -1:
            return
        self.pref_height = min(self.pref_height, self.max_height)

    def _draw(self, surface: pg.Surface, context: Rect, configs: WindowConfigs) -> tuple[int, int]:
        bounds = context.copy()

        result = self.draw(surface, bounds, configs)
        self.post_draw(surface, context, configs)
        return result

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        pass

    def post_draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        mx, my = pg.mouse.get_pos()
        if bounds.x <= mx <= bounds.x + bounds.width and bounds.y <= my <= bounds.y + bounds.height:
            for cc in self.click_configs:
                if cc.mouse_over_condition is not None and cc.mouse_over_condition():
                    cc.mouse_over_action(surface, bounds)
                    if configs.mouse_clicked and cc.mouse_click_condition is not None and cc.mouse_click_condition():
                        cc.mouse_click_action()

    def bounds(self, context: Rect) -> Rect:
        width = between(self.get_min_width(), context.width, self.get_max_width())
        height = between(self.get_min_height(), context.height, self.get_max_height())
        return Rect(self.x, self.y, width, height)
    
    def move(self, x: int, y: int):
        self.x, self.y = x, y

    def get_pref_width(self):
        return self.pref_width
    
    def get_pref_height(self):
        return self.pref_height
    
    def load(self, data):
        pass


class Container(Widget):
    def __init__(self, outline_color: tuple[int, int, int]=None):
        super().__init__()

        self.outline_color = outline_color
        self.widgets: list[Widget] = []

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        for widget in self.widgets:
            width = widget.get_pref_width()
            height = widget.get_pref_height()
            b = Rect(bounds.x + widget.x, bounds.y + widget.y, width, height)
            widget._draw(surface, b, configs)
        return bounds.width, bounds.height

    def add_widget(self, w: Widget):
        self.widgets += [w]
        w.parent = self


class RectWidget(Widget):
    def __init__(self, bg_color: tuple[int, int, int]=WHITE, pref_width: int=50, pref_height: int=50, min_width: int=0, min_height: int=0, max_width: int=-1, max_height: int=-1, click_configs: list[ClickConfig]=None):
        super().__init__(pref_width, pref_height, min_width, min_height, max_width, max_height, click_configs)

        self.bg_color = bg_color

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        pg.draw.rect(surface, self.bg_color, (bounds.x, bounds.y, bounds.width, bounds.height))
        
        if configs.wireframe:
            pg.draw.rect(surface, BLACK, (bounds.x, bounds.y, bounds.width, bounds.height), 1)
            pg.draw.line(surface, BLACK, (bounds.x, bounds.y), (bounds.x + bounds.width - 1, bounds.y + bounds.height - 1))
            pg.draw.line(surface, BLACK, (bounds.x, bounds.y + bounds.height - 1), (bounds.x + bounds.width - 1, bounds.y))

        # pg.display.flip()
        # pg.time.wait(60)
        return bounds.width, bounds.height
SPACE_FILLER = RectWidget()


class LabelComponent:
    def __init__(self, font: pg.font.Font, text: str, fg_color: tuple[int, int, int]=BLACK):
        self.label: pg.sprite.Sprite = None
        self.fg_color = fg_color

        self.font = font
        if text == '':
            return
        self.set_text(text)

    def set_text(self, text: str):
        self.label = self.font.render(text, False, self.fg_color)


class LabelWidget(RectWidget, LabelComponent):

    def __init__(self, font: pg.font.Font, text: str, fg_color: tuple[int, int, int]=BLACK, bg_color: tuple[int, int, int] = WHITE, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1, click_configs: list[ClickConfig]=None):
        RectWidget.__init__(self, bg_color, pref_width, pref_height, min_width, min_height, max_width, max_height, click_configs)
        LabelComponent.__init__(self, font, text, fg_color)

    def set_text(self, text: str):
        LabelComponent.set_text(self, text)

        self.min_width = self.label.get_width()
        self.min_height = self.label.get_height()
        
        # TODO? remove
        # self.max_width = self.label.get_width()
        self.max_height = self.label.get_height()

        self.pref_width = self.label.get_width()
        self.pref_height = self.label.get_height()

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        super().draw(surface, bounds, configs)
        if self.label is not None:
            surface.blit(self.label, bounds.to_tuple())
        return (bounds.width, bounds.height)


class FlowConfig(Container):
    def __init__(self):
        self.dominant = ''

    def extract_value_from_bounds(self, bounds: Rect):
        # return bounds.width
        raise NotImplementedError()
    
    def extract_pref_value(self, widget: Widget):
        # return widget.get_pref_width()
        raise NotImplementedError()
    
    def extract_min_value(self, widget: Widget):
        # return widget.get_pref_width()
        raise NotImplementedError()
    
    def extract_max_value(self, widget: Widget):
        # return widget.get_pref_width()
        raise NotImplementedError()
    
    def config_other_bound(self, me: Widget, bounds: Rect):
        # bounds.height = between(me.get_min_height(), me.get_pref_height(), me.get_max_height())
        raise NotImplementedError()

    def mutate_coords(self, bounds: Rect, width: int, height: int):
        # bounds.x += width
        raise NotImplementedError()
    
    def set_bound(self, bounds: Rect, value: int):
        # bounds.x = value
        raise NotImplementedError()
    
    def select_important(self, width: int, height: int):
        # return width
        raise NotImplementedError()
    
    def pick_return_values(self, bounds: Rect, sum_width: int, sum_height: int):
        # return sum_width, bounds.height
        raise NotImplementedError()
    
    def pref_width(self, arr: list[Widget]):
        # return sum(arr)
        raise NotImplementedError()
    
    def pref_height(self, arr: list[Widget]):
        # return sum(arr)
        raise NotImplementedError()
    
    def min_width(self, arr: list[Widget]):
        # return sum(arr)
        raise NotImplementedError()
    
    def min_height(self, arr: list[Widget]):
        # return sum(arr)
        raise NotImplementedError()
        

class FlowContainer(Container):
    def __init__(self, flow_config: FlowConfig, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.config = flow_config

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        b = bounds.copy()
        value = self.config.extract_value_from_bounds(bounds)
        # count = len(self.widgets)
        # per_one = value / count
        sum_w = 0
        sum_h = 0

        # TODO figure out how to fit in extract_pref_value, works better than extract_max_value
        sizes = size_config(
            self.widgets, 
            value, 
            lambda w: self.config.extract_min_value(w), 
            lambda w: self.config.extract_max_value(w)
            # lambda w: self.config.extract_pref_value(w)
        )
            

        '''
        for widget in self.widgets:
            min_s = self.config.extract_min_value(widget)
            pref = self.config.extract_pref_value(widget)
            max_s = self.config.extract_max_value(widget)

            size = pref
            
            sizes += [size]
            if size > per_one:
                value -= size
                count -= 1
                if count != 0:
                    per_one = value / count
        '''

        for i in range(len(self.widgets)):
            widget = self.widgets[i]
            size = sizes[i]
            self.config.set_bound(b, size)
            res_w, res_h = widget._draw(surface, b, configs)

            self.config.mutate_coords(b, res_w, res_h)
            sum_h += res_h
            sum_w += res_w
        
        if configs.wireframe:
            pg.draw.rect(surface, BLACK, bounds.to_tuple(), 2)
            pg.draw.line(surface, BLACK, (bounds.x, bounds.y), (bounds.x + bounds.width - 1, bounds.y + bounds.height - 1))
            pg.draw.line(surface, BLACK, (bounds.x, bounds.y + bounds.height - 1), (bounds.x + bounds.width - 1, bounds.y))
            # pg.draw.line(surface, BLACK, (start_x, start_y + b.height - 1), (start_x + sum_w - 1, start_y))
        
        return self.config.pick_return_values(bounds, sum_w, sum_h)

    def get_min_width(self):
        return self.config.min_width(self.widgets)
    
    def get_min_height(self):
        return self.config.min_height(self.widgets)
    
    def get_pref_width(self):
        return self.config.pref_width(self.widgets)
    
    def get_pref_height(self):
        return self.config.pref_height(self.widgets)
    
    def min_metric_accumulator(self, extractor):
        result = None
        for widget in self.widgets:
            mv = extractor(widget)
            if mv != -1 and (result == None or mv < result):
                result = mv
        return result if result is not None else -1
    
    def sum_metric_accumulator(self, extractor):
        result = 0
        for widget in self.widgets:
            mv = extractor(widget)
            if mv == -1:
                return -1
            result += mv
        if result == 0:
            return -1
        return result

    def get_max_height(self):
        if self.config.dominant == 'height':
            return self.sum_metric_accumulator(lambda w: w.get_max_height())
        return self.min_metric_accumulator(lambda w: w.get_max_height())
    
    def get_max_width(self):
        if self.config.dominant == 'height':
            return self.min_metric_accumulator(lambda w: w.get_max_width())
        return self.sum_metric_accumulator(lambda w: w.get_max_width())
    

class HorFlowConfig(FlowConfig):
    def __init__(self):
        super().__init__()

        self.dominant = 'width'

    def extract_value_from_bounds(self, bounds: Rect):
        return bounds.width
    
    def extract_pref_value(self, widget: Widget):
        return widget.get_pref_width()
    
    def extract_min_value(self, widget: Widget):
        return widget.get_min_width()
    
    def extract_max_value(self, widget: Widget):
        return widget.get_max_width()
    
    def config_other_bound(self, me: Widget, bounds: Rect):
        bounds.height = between(me.get_min_height(), me.get_pref_height(), me.get_max_height())

    def mutate_coords(self, bounds: Rect, width: int, height: int):
        bounds.x += width
    
    def set_bound(self, bounds: Rect, value: int):
        bounds.width = value
    
    def select_important(self, width: int, height: int):
        return width
    
    def pick_return_values(self, bounds: Rect, sum_width: int, sum_height: int):
        return sum_width, bounds.height
            
    def min_width(self, arr: list[Widget]):
        return sum([w.get_min_width() for w in arr])
    
    def min_height(self, arr: list[Widget]):
        return max([w.get_min_height() for w in arr])
    
    def pref_width(self, arr: list[Widget]):
        a = []
        for w in arr:
            v = w.get_pref_width()
            a += [v]
        return sum(a)
    
    def pref_height(self, arr: list[Widget]):
        if len(arr) == 1:
            return arr[0].get_pref_height()
        prefs = []
        mins = []
        maxs = []
        for w in arr:
            w_min = w.get_min_height()
            w_max = w.get_max_height()
            w_pref = w.get_pref_height()
            prefs += [w_pref]
            mins += [w_min]
            maxs += [w_max]
        mi = max(mins)
        ma = min(maxs)
        result = []
        for pref in prefs:
            if mi <= pref <= ma or (mi <= pref and ma == -1):
                result += [pref]
        if len(result) == 0:
            raise Exception('FAILED TO GET PREF HEIGHT')
        return max(result)


class VerFlowConfig(FlowConfig):
    def __init__(self):
        super().__init__()

        self.dominant = 'height'

    def extract_value_from_bounds(self, bounds: Rect):
        return bounds.height
    
    def extract_pref_value(self, widget: Widget):
        return widget.get_pref_height()
    
    def extract_min_value(self, widget: Widget):
        return widget.get_min_height()
    
    def extract_max_value(self, widget: Widget):
        return widget.get_max_height()
    
    def config_other_bound(self, me: Widget, bounds: Rect):
        bounds.width = between(me.get_min_width(), me.get_pref_width(), me.get_max_width())

    def mutate_coords(self, bounds: Rect, width: int, height: int):
        bounds.y += height
    
    def set_bound(self, bounds: Rect, value: int):
        bounds.height = value
    
    def select_important(self, width: int, height: int):
        return height
    
    def pick_return_values(self, bounds: Rect, sum_width: int, sum_height: int):
        return bounds.width, sum_height
    
    def min_width(self, arr: list[Widget]):
        return max([w.get_min_width() for w in arr])
    
    def min_height(self, arr: list[Widget]):
        return sum([w.get_min_height() for w in arr])
    
    def pref_width(self, arr: list[Widget]):
        if len(arr) == 1:
            return arr[0].get_pref_width()
        prefs = []
        mins = []
        maxs = []
        for w in arr:
            w_min = w.get_min_width()
            w_max = w.get_max_width()
            w_pref = w.get_pref_width()
            prefs += [w_pref]
            mins += [w_min]
            maxs += [w_max]
        mi = max(mins)
        ma = min(maxs)
        result = []
        for pref in prefs:
            if mi <= pref <= ma or (mi <= pref and ma == -1):
                result += [pref]
        if len(result) == 0:
            raise Exception('FAILED TO GET PREF HEIGHT')
        return max(result)
    
    def pref_height(self, arr: list[Widget]):
        return sum([w.get_pref_height() for w in arr])


class HorContainer(FlowContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(HorFlowConfig(), outline_color)


class VerContainer(FlowContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(VerFlowConfig(), outline_color)


class ButtonWidget(RectWidget):
    def __init__(self, font: pg.font.Font=None, text: str='', fg_color: tuple[int, int, int] = BLACK, bg_color: tuple[int, int, int] = WHITE, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1):
        cc = ClickConfig()

        cc.mouse_over_condition = lambda: True
        cc.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc.mouse_click_condition = lambda: True
        cc.mouse_click_action = self.click_action

        click_configs = [
            cc
        ]

        LabelComponent.__init__(self, font, text, fg_color)
        Widget.__init__(self, bg_color, pref_width, pref_height, min_width, min_height, max_width, max_height, click_configs)

        self.click = None

    def click_action(self):
        if self.click is None:
            return
        
        self.click()

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        result = RectWidget.draw(self, surface, bounds, configs)

        if self.label:
            lb = bounds.copy()
            lb.x += (lb.width - self.label.get_width()) // 2
            lb.y += (lb.height - self.label.get_height()) // 2
            surface.blit(self.label, lb.to_tuple())

        return result
    
    def set_text(self, text: str):
        LabelComponent.set_text(self, text)

        self.min_width = self.label.get_width()
        self.min_height = self.label.get_height()


class FormContainer(VerContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

    def add_pair(self, label: LabelWidget, widget: Widget):
        c = HorContainer()
        c.add_widget(label)
        c.add_widget(widget)
        self.add_widget(c)


class StackContainer(VerContainer):
    def __init__(self, max_per_line: int, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

        self.max_per_line = max_per_line
        self.last_container: HorContainer = None
        self.widgets += [SPACE_FILLER]

    def add_widget(self, w: Widget):
        if self.last_container is None or len(self.last_container.widgets) == self.max_per_line + 1:
            self.last_container = HorContainer()
            self.last_container.add_widget(SPACE_FILLER)
            self.widgets.insert(len(self.widgets)-1, self.last_container)
        self.last_container.widgets.insert(len(self.last_container.widgets)-1, w)
            

class ScrollWidget(RectWidget):
    # def __init__(self, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1, click_configs: list[ClickConfig] = None):
    #     super().__init__(pref_width, pref_height, min_width, min_height, max_width, max_height, click_configs)
    def __init__(self, bg_color: tuple[int, int, int] = WHITE, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1, click_configs: list[ClickConfig] = None):
        super().__init__(bg_color, pref_width, pref_height, min_width, min_height, max_width, max_height, click_configs)

        self.scroll = 0
        self.widget: Widget = None

    def set_widget(self, w: Widget):
        self.widget = w

    def _draw(self, surface: pg.Surface, context: Rect, configs: WindowConfigs) -> tuple[int, int]:
        return super()._draw(surface, context, configs)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        # self.widget.draw(surface, bounds, configs)
        pwidth = self.widget.get_pref_width()
        pheight = self.widget.get_pref_height()
        surf = pg.Surface((bounds.width, bounds.height))
        super().draw(surf, Rect(0, 0, bounds.width, bounds.height), configs)
        self.widget._draw(surf, Rect(0, 0+self.scroll, pwidth, pheight), configs)
        surface.blit(surf, (bounds.x, bounds.y))
        return bounds.width, bounds.height
        # return super().draw(surface, bounds, configs)













# w = Window()
# w.set_title('client test')
# # w.container = VertContainer()
# c = w.container


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


# # front layout
# container = VerContainer()
# container.add_widget(RectWidget(BLUE))
# container.add_widget(RectWidget(RED))
# w.container = container
# w.run()

