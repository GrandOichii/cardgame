# import client

# client.Client.INSTANCE = client.Client()
# client.Client.INSTANCE.start()

'''
Singleton template:
class Singleton(object):
    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(Singleton, cls).__new__(cls)
        return cls.instance
'''

import random
# random.seed(0)
import pygame as pg

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

    def toggle_wireframe(self):
        self.wireframe = not self.wireframe

# new front architecture
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

    def events(self):
        for event in pg.event.get():
            if event.type == pg.QUIT:
                self.running = False
            if event.type == pg.KEYDOWN:
                self.process_key(event)

    def process_key(self, event: pg.event.Event):
        key = event.key
        if not key in self.key_map: return

        self.key_map[key]()


class ContentLoader:
    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(ContentLoader, cls).__new__(cls)
        return cls.instance


def between(min: int, cur: int, max: int) -> int:
    if cur < min:
        return min
    if max == -1:
        return cur
    if cur > max:
        return max
    return cur


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
        return f'{self.x} {self.y} {self.width} {self.height}'
    
    def with_mod(self, xmod: int=0, ymod: int=0, widthmod: int=0, heightmod: int=0):
        return Rect(self.x + xmod, self.y + ymod, self.width + widthmod, self.height + heightmod)


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
    
    def set_pref_width(self, value: str):
        self.pref_width = max(value, self.min_width)

    def set_pref_height(self, value: str):
        self.pref_height = max(value, self.min_height)

    def _draw(self, surface: pg.Surface, context: Rect, fit_type: int, configs: WindowConfigs) -> tuple[int, int]:
        bounds = context.copy()

        if fit_type != 1:
            bounds.height = self.get_pref_height()
            # bounds.height = context.height - context.y
            
        if fit_type != 2:
            bounds.width = self.get_pref_width()
            # bounds.width = context.width - context.x

        bounds.width = between(self.get_min_width(), context.width, self.max_width)
        bounds.height = between(self.get_min_height(), context.height, self.max_height)
        return self.draw(surface, bounds, configs)

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        pass

    def bounds(self, context: Rect) -> Rect:
        width = between(self.get_min_width(), context.width, self.max_width)
        height = between(self.get_min_height(), context.height, self.max_height)
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
            widget._draw(surface, bounds.with_mod(xmod=widget.x, ymod=widget.y), 0, configs)
        return bounds.width, bounds.height

    def add_widget(self, w: Widget):
        self.widgets += [w]
        w.parent = self

    # def bounds(self) -> Rect:
    #     # TODO
    #     return super().bounds()


class RectWidget(Widget):
    def __init__(self, color: tuple[int, int, int], pref_width: int=50, pref_height: int=50, min_width: int=0, min_height: int=0, max_width: int=-1, max_height: int=-1):
        super().__init__(pref_width, pref_height, min_width, min_height, max_width, max_height)

        self.color = color

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        pg.draw.rect(surface, self.color, (bounds.x, bounds.y, bounds.width, bounds.height))
        
        if configs.wireframe:
            pg.draw.rect(surface, BLACK, (bounds.x, bounds.y, bounds.width, bounds.height), 1)
            pg.draw.line(surface, BLACK, (bounds.x, bounds.y), (bounds.x + bounds.width - 1, bounds.y + bounds.height - 1))
            pg.draw.line(surface, BLACK, (bounds.x, bounds.y + bounds.height - 1), (bounds.x + bounds.width - 1, bounds.y))

        # ??? WHY DOES THIS BREAK ???
        # if Window().configs.wireframe:
        #     print('a')

        # pg.display.flip()
        # pg.time.wait(60)
        return bounds.width, bounds.height
    

class HorContainer(Container):
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
            size = 0
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

        b.height = between(self.get_min_height(), self.get_pref_height(), self.max_height)
        for i in range(len(self.widgets)):
            widget = self.widgets[i]
            b.width = per_one
            size = sizes[i]
            if size > per_one:
                b.width = size
            res_w, res_h = widget._draw(surface, b, 1, configs)
            b.x += res_w
            sum += res_w
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
    
    
class VerContainer(Container):
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

        b.width = between(self.get_min_width(), self.get_pref_width(), self.max_width)
        for i in range(len(self.widgets)):
            widget = self.widgets[i]
            b.height = per_one
            size = sizes[i]
            if size > per_one:
                b.height = size
            res_w, res_h = widget._draw(surface, b, 2, configs)
            b.y += res_h
            sum += res_h
        return bounds.width, sum
    
    def get_min_width(self):
        return max([widget.get_min_width() for widget in self.widgets])
    
    def get_min_height(self):
        return sum([widget.get_min_height() for widget in self.widgets])
    
    def get_pref_width(self):
        # TODO
        return max([widget.get_pref_width() for widget in self.widgets])
    
    def get_pref_height(self):
        return sum([widget.get_pref_height() for widget in self.widgets])
    


w = Window()
w.set_title('client test')
# w.container = VertContainer()
c = w.container

COLORS = [
    RED,
    LRED,
    GREEN,
    LGREEN,
    BLUE,
    LBLUE,
    GRAY,
    LGRAY,
]

def random_widget():
    return RectWidget(random_color(), min_width=random.randint(10, 100), min_height=random.randint(10, 100))

PREV_COLOR = [0, 255, 255]
def next_color():
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
    if level % 2 == 0:
        new = VerContainer()
    c.add_widget(new)
    new.add_widget(w)
    add(new, level-1)
    if level % 4 == 0 or level % 4 == 1:
        new.widgets = new.widgets[::-1]

    # new.add_widget(w)

add(c, 20)

# ww = RectWidget(RED)
# ww.move(200, 300)
# c.add_widget(ww)
    

# c1 = VertContainer()
# c1.move(100, 100)
# c.add_widget(c1)


# c2 = HorContainer()
# c2.add_widget(RectWidget(random_color()))
# c2.add_widget(RectWidget(random_color()))

# c1.add_widget(RectWidget(random_color()))
# c1.add_widget(c2)
# c1.add_widget(RectWidget(random_color()))




# w1 = RectWidget(RED)
# w.container.add_widget(w1)
# w1.move(10, 10)

# c1 = VertContainer()
# c1.add_widget(RectWidget(BLUE))
# c1.add_widget(RectWidget(GREEN))
# c1.add_widget(RectWidget(LBLUE))
# c1.move(100, 300)

# c2 = HorContainer()
# c2.max_height = 100
# c2.max_width = 100
# c2.move(200, 0)
# c2.add_widget(RectWidget(RED))
# c2.add_widget(RectWidget(BLUE))

# c.add_widget(c1)
# c.add_widget(c2)
# w.go_fullscreen()
print(w.configs)
print(WindowConfigs())
w.run()