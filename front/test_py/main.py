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

G_INDENT = 0
def iprint(str):
    print('-'*G_INDENT, str, sep='')

def random_color():
    return (random.randint(0, 255), random.randint(0, 255), random.randint(0, 255))

# new front architecture
class Window:
    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(Window, cls).__new__(cls)
        return cls.instance
    
    def __init__(self):
        pg.init()
        self.fps = 60
        self.set_screen_size(800, 600)

        self.clock = pg.time.Clock()

        self.running = True

        self.container = Container()


    def set_title(self, title: str):
        pg.display.set_caption(title)

    def go_fullscreen(self, value):
        if self.fullscreen == value:
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
        self.container._draw(self.screen, Rect.from_tuple((0, 0, self.width, self.height)), 0)

    def events(self):
        for event in pg.event.get():
            if event.type == pg.QUIT:
                self.running = False


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
    def __init__(self, x=0, y=0, height=0, width=0) -> None:
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


class Widget:
    def __init__(self):
        self.parent: Container = None
        self.x, self.y = 0, 0
        self.min_width, self.max_width, self.pref_width = 0, -1, 100
        self.min_height, self.max_height, self.pref_height = 0, -1, 100

    def get_min_width(self):
        return self.min_width
    
    def get_min_height(self):
        return self.min_height

    def _draw(self, surface: pg.Surface, context: Rect, fit_type: int) -> tuple[int, int]:
        bounds = context.copy()
        # bounds.width = between(self.get_min_width(), bounds.width, self.max_width)
        # bounds.height = between(self.get_min_height(), bounds.height, self.max_height)
        
        # freeform
        if fit_type == 0:
            bounds.x += self.x
            bounds.y += self.y

        # fit vert
        if fit_type == 1:
            bounds.height = self.get_pref_height(context)

        # fir hor
        if fit_type == 2:
            bounds.width = self.get_pref_width(context)


        return self.draw(surface, bounds)

    def draw(self, surface: pg.Surface, bounds: Rect) -> tuple[int, int]:
        pass

    def bounds(self, context: Rect) -> Rect:
        width = between(self.get_min_width(), context.width, self.max_width)
        height = between(self.get_min_height(), context.height, self.max_height)
        return Rect(self.x, self.y, width, height)
    
    def move(self, x: int, y: int):
        self.x, self.y = x, y

    def get_pref_width(self, context: Rect):
        return self.pref_width
    
    def get_pref_height(self, context: Rect):
        return self.pref_height
    

# simple freeform container
class Container(Widget):
    def __init__(self, outline_color: tuple[int, int, int]=None):
        super().__init__()

        self.outline_color = outline_color
        self.widgets: list[Widget] = []

    def draw(self, surface: pg.Surface, bounds: Rect):
        for widget in self.widgets:
            widget._draw(surface, bounds, 1)
        return bounds.width, bounds.height

    def add_widget(self, w: Widget):
        self.widgets += [w]
        w.parent = self

    # def bounds(self) -> Rect:
    #     # TODO
    #     return super().bounds()


class RectWidget(Widget):
    def __init__(self, color: tuple[int, int, int],  width: int=10, height: int=10):
        super().__init__()

        self.color = color
        self.height = height
        self.width = width

    def draw(self, surface: pg.Surface, bounds: Rect):
        pg.draw.rect(surface, self.color, (bounds.x, bounds.y, bounds.width, bounds.height))
        return bounds.width, bounds.height
    

class VertContainer(Container):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

    def draw(self, surface: pg.Surface, bounds: Rect):
        iprint(bounds)
        global G_INDENT
        G_INDENT += 1
        b = bounds.copy()
        per_one = bounds.width / len(self.widgets)
        sum = 0
        for widget in self.widgets:
            if widget.get_min_width() > per_one:
                per_one -= widget.get_min_width() - per_one
        for widget in self.widgets:
            b.width = per_one
            if widget.get_min_width() > per_one:
                b.width = widget.get_min_width()
            res_w, res_h = widget._draw(surface, b, 2)
            b.x += res_w
            sum += res_w
        G_INDENT -= 1
        return sum, bounds.height
    
    def get_min_width(self):
        return max(self.min_width, sum([widget.get_min_width() for widget in self.widgets]))
    
    def get_min_height(self):
        return max(self.min_height, max([widget.get_min_height() for widget in self.widgets]))
    
    def get_pref_width(self, context: Rect):
        return max(self.get_min_width(), context.width)
    
    def get_pref_height(self, context: Rect):
        return max(self.get_min_height(), context.height)
    

 

class HorContainer(Container):
    def __init__(self, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)

    def draw(self, surface: pg.Surface, bounds: Rect):
        b = bounds.copy()
        per_one = bounds.height / len(self.widgets)
        for widget in self.widgets:
            if widget.get_min_height() > per_one:
                per_one -= widget.get_min_height() - per_one
        for widget in self.widgets:
            b.height = per_one
            if widget.get_min_height() > per_one:
                b.height = widget.get_min_height()
            res_w, res_h = widget._draw(surface, b, True)
            b.y += res_h
        return bounds.width, bounds.height

    def get_min_width(self):
        return max(self.min_width, max([widget.get_min_width() for widget in self.widgets]))
    
    def get_min_height(self):
        return max(self.min_height, sum([widget.get_min_height() for widget in self.widgets]))
    
    def get_pref_width(self, context: Rect):
        return max(self.get_min_width(), context.width)
    
    def get_pref_height(self, context: Rect):
        return max(self.get_min_height(), context.height)

w = Window()
w.set_title('client test')
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

def add(c: Container, level: int):
    w = RectWidget(COLORS[level])
    c.add_widget(w)
    if level == 0:
        return None
    
    new = VertContainer()
    # if level % 2 == 0:
    #     new = HorContainer()
    c.add_widget(new)
    add(new, level-1)

add(c, 4)
    



# c1 = VertContainer()
# c1.move(100, 100)
# c.add_widget(c1)


# c2 = HorContainer()
# c2.add_widget(RectWidget(random_color()))
# c2.add_widget(RectWidget(random_color()))

# c1.add_widget(RectWidget(random_color()))
# c1.add_widget(c2)




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
w.run()