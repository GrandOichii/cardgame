# import client

# client.Client.INSTANCE = client.Client()
# client.Client.INSTANCE.start()


import os
import random
# random.seed(0)
import pygame as pg
from sizeconfig import size_config


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
    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(Window, cls).__new__(cls)
        return cls.instance
    
    def __init__(self):
        pg.init()
        self.configs = WindowConfigs()


        self.key_map = {
            pg.K_SPACE: self.configs.toggle_wireframe
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

            # draw
            self.draw()

            # refresh screen
            pg.display.flip()

            # events
            self.events()

    def draw(self):
        self.screen.fill(WHITE)
        self.container._draw(self.screen, Rect(0, 0, -1, -1), 0, self.configs)
        self.configs.mouse_clicked = False

    def events(self):
        for event in pg.event.get():
            if event.type == pg.QUIT:
                self.running = False
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


class Widget:
    def __init__(self, pref_width: int=50, pref_height: int=50, min_width: int=0, min_height: int=0, max_width: int=-1, max_height: int=-1):
        self.parent: Container = None
        self.x, self.y = 0, 0
        self.min_width, self.max_width = min_width, max_width
        self.min_height, self.max_height = min_height, max_height
        
        self.set_pref_width(pref_width)
        self.set_pref_height(pref_height)

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

    def _draw(self, surface: pg.Surface, context: Rect, fit_type: int, configs: WindowConfigs) -> tuple[int, int]:
        bounds = context.copy()

        # if fit_type != 1:
        #     bounds.height = self.get_pref_height()
        #     # bounds.height = context.height - context.y
            
        # if fit_type != 2:
        #     bounds.width = self.get_pref_width()
        #     # bounds.width = context.width - context.x

        # new_var = self.get_min_width()
        # new_var1 = min(self.get_max_width(), context.width)
        # if new_var1 == -1:
        #     new_var1 = context.width
        # bounds.width = between(new_var, bounds.width, new_var1)
        # new_var2 = self.get_min_height()
        # new_var3 = min(self.get_max_height(), context.height)
        # if new_var3 == -1:
        #     new_var3 = context.height
        # bounds.height = between(new_var2, bounds.height, new_var3)

        # print(self, bounds, sep='\t')
        return self.draw(surface, bounds, configs)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        pass

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
    

class Container(Widget):
    def __init__(self, outline_color: tuple[int, int, int]=None):
        super().__init__()

        self.outline_color = outline_color
        self.widgets: list[Widget] = []

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        for widget in self.widgets:
            b = Rect(bounds.x + widget.x, bounds.y + widget.y, widget.get_pref_width(), widget.get_pref_height())
            widget._draw(surface, b, 0, configs)
        return bounds.width, bounds.height

    def add_widget(self, w: Widget):
        self.widgets += [w]
        w.parent = self

    # def bounds(self) -> Rect:
    #     # TODO
    #     return super().bounds()


class RectWidget(Widget):
    def __init__(self, bg_color: tuple[int, int, int]=WHITE, pref_width: int=50, pref_height: int=50, min_width: int=0, min_height: int=0, max_width: int=-1, max_height: int=-1):
        super().__init__(pref_width, pref_height, min_width, min_height, max_width, max_height)

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

    def __init__(self, font: pg.font.Font, text: str, fg_color: tuple[int, int, int]=BLACK, bg_color: tuple[int, int, int] = WHITE, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1):
        RectWidget.__init__(self, bg_color, pref_width, pref_height, min_width, min_height, max_width, max_height)
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

        surface.blit(self.label, bounds.to_tuple())
        return (bounds.width, bounds.height)


'''
class HorContainer1(Container):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        b = bounds.copy()
        width = bounds.width
        count = len(self.widgets)
        per_one = width / count
        sum = 0
        sizes = []
        for widget in self.widgets:
            size = widget.get_pref_width()
            # if fit_type == 0:
            #     size = widget.get_pref_width()
            # else:
            #     size = widget.get_min_width()

            sizes += [size]
            if size > per_one:
                width -= size
                count -= 1
                if count != 0:
                    per_one = width / count

        b.height = between(self.get_min_height(), self.get_pref_height(), self.get_max_height())
        for i in range(len(self.widgets)):
            widget = self.widgets[i]
            b.width = per_one
            size = sizes[i]
            if size > per_one:
                b.width = size
            res_w, res_h = widget._draw(surface, b, 1, configs)
            if res_w < per_one:
                width -= res_w
                count -= 1
                if count == 0:
                    break
                per_one = width / count
            b.x += res_w
            sum += res_w
        
        # if configs.wireframe:
        #     pg.draw.rect(surface, BLACK, (startx, b.y, sum, b.height), 2)
        #     pg.draw.line(surface, BLACK, (startx, b.y), (startx + sum - 1, b.y + b.height - 1))
        #     pg.draw.line(surface, BLACK, (startx, b.y + b.height - 1), (startx + sum - 1, b.y))
        

        return sum, bounds.height
    
    def get_min_width(self):
        return sum([widget.get_min_width() for widget in self.widgets])
    
    def get_min_height(self):
        return max([widget.get_min_height() for widget in self.widgets])
    
    def get_pref_width(self):
        return sum([widget.get_pref_width() for widget in self.widgets])
    
    def get_pref_height(self):
        # TODO
        return max([widget.get_pref_height() for widget in self.widgets])
    
    def get_max_height(self):
        # result = 0
        # for widget in self.widgets:
        #     mw = widget.get_max_height()
        #     if mw == -1:
        #         continue
        #     result += mw
        # if result == 0:
        #     return -1
        # return result
        result = None
        for widget in self.widgets:
            mw = widget.get_max_height()
            if mw != -1 and (result == None or mw < result):
                result = mw
        return result if result is not None else -1
    
    def get_max_width(self):
        result = 0
        for widget in self.widgets:
            mw = widget.get_max_width()
            if mw == -1:
                return -1
            result += mw
        if result == 0:
            return -1
        return result
    

class VerContainer1(Container):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        b = bounds.copy()
        height = bounds.height
        count = len(self.widgets)
        if count == 0:
            return 0, 0
        per_one = height / count
        sum = 0
        sizes = []
        for widget in self.widgets:
            size = 0
            size = widget.get_pref_height()
            # if fit_type == 0:
            #     size = widget.get_pref_height()
            # else:
            #     size = widget.get_min_height()

            sizes += [size]
            if size > per_one:
                height -= size
                count -= 1
                if count != 0:
                    per_one = height / count

        new_var = self.get_min_width()
        new_var1 = self.get_pref_width()
        new_var2 = self.get_max_width()
        b.width = between(new_var, new_var1, new_var2)
        for i in range(len(self.widgets)):
            widget = self.widgets[i]
            b.height = per_one
            size = sizes[i]
            if size > per_one:
                b.height = size
            res_w, res_h = widget._draw(surface, b, 2, configs)
            if res_h < per_one:
                height -= res_h
                count -= 1
                if count == 0:
                    break
                per_one = height / count
            b.y += res_h
            sum += res_h
        
        # if configs.wireframe:
        #     pg.draw.rect(surface, BLACK, (b.x, starty, b.width, sum), 2)
        #     pg.draw.line(surface, BLACK, (b.x, starty), (b.x + b.width - 1, starty + sum - 1))
        #     pg.draw.line(surface, BLACK, (b.x, starty + sum - 1), (b.x + b.width - 1, starty))

        return bounds.width, sum
    
    def get_min_width(self):
        return max([widget.get_min_width() for widget in self.widgets])
    
    def get_min_height(self):
        return sum([widget.get_min_height() for widget in self.widgets])
    
    def get_pref_width(self):
        return max([widget.get_pref_width() for widget in self.widgets])
    
    def get_pref_height(self):
        return sum([widget.get_pref_height() for widget in self.widgets])
    
    def get_max_width(self):
        # result = 0
        # for widget in self.widgets:
        #     mw = widget.get_max_width()
        #     if mw == -1:
        #         continue
        #     result += mw
        # if result == 0:
        #     return -1
        # return result
        result = None
        for widget in self.widgets:
            mw = widget.get_max_width()
            if mw != -1 and (result == None or mw < result):
                result = mw
        # return result
        return result if result is not None else -1
    
        # return min([widget.get_max_width() for widget in self.widgets])
    
    def get_max_height(self):
        result = 0
        for widget in self.widgets:
            mw = widget.get_max_height()
            if mw == -1:
                return -1
            result += mw
        if result == 0:
            return -1
        return result
'''


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
    
    def pref_and_min_policy_width(self, arr: list[Widget]):
        # return sum(arr)
        raise NotImplementedError()
    
    def pref_and_min_policy_height(self, arr: list[Widget]):
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
        sizes = size_config(
            self.widgets, 
            value, 
            lambda w: self.config.extract_min_value(w), 
            lambda w: self.config.extract_max_value(w)
        )
        print(sizes)

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
            res_w, res_h = widget._draw(surface, b, 1, configs)
            # i_metric = self.config.select_important(res_w, res_h)
            # if i_metric < per_one:
            #     value -= i_metric
            #     count -= 1
            #     if count == 0:
            #         break
            #     per_one = value / count

            # b.x += res_w
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
        return self.config.pref_and_min_policy_width([widget.get_min_width() for widget in self.widgets])
    
    def get_min_height(self):
        return self.config.pref_and_min_policy_height([widget.get_min_height() for widget in self.widgets])
    
    def get_pref_width(self):
        a = []
        for widget in self.widgets:
            w = widget.get_pref_width()
            a += [w]
        return self.config.pref_and_min_policy_width(a)
        # return self.config.pref_and_min_policy_width([widget.get_pref_width() for widget in self.widgets])
    
    def get_pref_height(self):
        # TODO
        return self.config.pref_and_min_policy_height([widget.get_pref_height() for widget in self.widgets])
    
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

        # if self.config.dominant == 'height':
            # return self.
    #     result = None
    #     for widget in self.widgets:
    #         mw = widget.get_max_height()
    #         if mw != -1 and (result == None or mw < result):
    #             result = mw
    #     return result if result is not None else -1
    
    def get_max_width(self):
        if self.config.dominant == 'height':
            return self.min_metric_accumulator(lambda w: w.get_max_width())
        return self.sum_metric_accumulator(lambda w: w.get_max_width())
    #     result = 0
    #     for widget in self.widgets:
    #         mw = widget.get_max_width()
    #         if mw == -1:
    #             return -1
    #         result += mw
    #     if result == 0:
    #         return -1
    #     return result
    

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
    
    def pref_and_min_policy_width(self, arr: list[Widget]):
        return sum(arr)
    
    def pref_and_min_policy_height(self, arr: list[Widget]):
        return max(arr)
    

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
    
    def pref_and_min_policy_width(self, arr: list[Widget]):
        return max(arr)
    
    def pref_and_min_policy_height(self, arr: list[Widget]):
        return sum(arr)


class HorContainer(FlowContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(HorFlowConfig(), outline_color)


class VerContainer(FlowContainer):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(VerFlowConfig(), outline_color)


class ClickConfig:
    def __init__(self):
        self.mouse_over_condition = None
        self.mouse_over_action = None
        
        self.mouse_click_condition = None
        self.mouse_click_action = None
        

class ButtonTemplateWidget(RectWidget, LabelComponent):
    def __init__(self, font: pg.font.Font, text: str, click_configs: list[ClickConfig], fg_color: tuple[int, int, int]=BLACK, bg_color: tuple[int, int, int] = WHITE, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1):
        RectWidget.__init__(self, bg_color, pref_width, pref_height, min_width, min_height, max_width, max_height)
        LabelComponent.__init__(self, font, text, fg_color)

        self.click_configs: list[ClickConfig] = click_configs

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        RectWidget.draw(self, surface, bounds, configs)

        # TODO change when text shouldn't be centered
        if self.label:
            lb = bounds.copy()
            lb.x += (lb.width - self.label.get_width()) // 2
            lb.y += (lb.height - self.label.get_height()) // 2
            surface.blit(self.label, lb.to_tuple())
        
        # lb.

        mx, my = pg.mouse.get_pos()
        if bounds.x <= mx <= bounds.x + bounds.width and bounds.y <= my <= bounds.y + bounds.height:
            for cc in self.click_configs:
                if cc.mouse_over_condition is not None and cc.mouse_over_condition():
                    cc.mouse_over_action(surface, bounds)
                    if configs.mouse_clicked and cc.mouse_click_condition is not None and cc.mouse_click_condition():
                        cc.mouse_click_action()

        return bounds.width, bounds.height
    
    def set_text(self, text: str):
        LabelComponent.set_text(self, text)

        self.min_width = self.label.get_width()
        self.min_height = self.label.get_height()


class ButtonWidget(ButtonTemplateWidget):
    def __init__(self, font: pg.font.Font=None, text: str='', fg_color: tuple[int, int, int] = BLACK, bg_color: tuple[int, int, int] = WHITE, pref_width: int = 50, pref_height: int = 50, min_width: int = 0, min_height: int = 0, max_width: int = -1, max_height: int = -1):
        cc = ClickConfig()

        cc.mouse_over_condition = lambda: True
        cc.mouse_over_action = lambda surface, bounds: pg.draw.rect(surface, RED, (bounds.x, bounds.y, bounds.width, bounds.height), 2)
        cc.mouse_click_condition = lambda: True
        cc.mouse_click_action = self.click_action

        click_configs = [
            cc
        ]

        super().__init__(font, text, click_configs, fg_color, bg_color, pref_width, pref_height, min_width, min_height, max_width, max_height)

        self.click = None

    def click_action(self):
        if self.click is None:
            return
        
        self.click()


w = Window()
w.set_title('client test')
# w.container = VertContainer()
c = w.container


def random_rect():
    return RectWidget(random_color(), min_width=random.randint(10, 100), min_height=random.randint(10, 100))

PREV_COLOR = [0, 255, 255]
def next_color():
    # return random_color()
    global PREV_COLOR
    PREV_COLOR[1] -= 10
    PREV_COLOR[2] -= 10
    return (PREV_COLOR[0], PREV_COLOR[1], PREV_COLOR[2])

def add(c: Container, level: int):
    w = RectWidget(next_color())
    # w = random_widget()
    # c.add_widget(w)
    if level == 0:
        return None
    
    new = HorContainer()
    # new = HorContainer1()
    if level % 2 == 0:
        new = VerContainer()
        # new = VerContainer1()
    c.add_widget(new)
    new.add_widget(w)
    add(new, level-1)
    if level % 4 == 0 or level % 4 == 1:
        new.widgets = new.widgets[::-1]

    # new.add_widget(w)

add(c, 15)



# ww = RectWidget(RED)
# ww.move(200, 300)
# c.add_widget(ww)
    
# ContentPool.Instance.load_font('basic', '/Users/oichii/Desktop/cardgame/front/test_py/fonts/Montserrat-Thin.ttf')

# TODO remove, for easier debugging
font = None
try:
    ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
    font = ContentPool.Instance.get_font('basic', 12)
except:    
    ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
    font = ContentPool.Instance.get_font('basic', 12)



def click():
    print('AA')


container = VerContainer()
container.move(10, 10)


entries_count = 3

# TODO don't fit, fix
up_button = ButtonWidget(bg_color=LGREEN, max_height=20)
container.add_widget(up_button)
for i in range(entries_count):
    c = HorContainer()
    left = RectWidget(LBLUE, max_width=20, max_height=20)
    r = 'a'*(5 * (i+1))
    # r = 'a'*(random.randint(1, 20))
    # r = 'aaa'
    right = LabelWidget(font, r, bg_color=LRED)
    # right = RectWidget(bg_color=RED)
    c.add_widget(left)
    c.add_widget(right)
    container.add_widget(c)

down_button = ButtonWidget(bg_color=LGREEN, max_height=20)
container.add_widget(down_button)
container.move(200, 200)
# w.container.add_widget(container)

w.run()