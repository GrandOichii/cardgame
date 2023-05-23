from math import cos, sin, pi, atan2
import pygame as pg
from front.test_py.frame import *
import random

CHARS = 'abcdefghikklmnopqrstuvwxyz1234567890?!-+='
def random_string():
    size = random.randint(13, 50)
    result = ''
    for i in range(size):
        result += CHARS[random.randint(0, len(CHARS)-1)]
    return result



class TestWindow(Window):
    def __init__(self):
        super().__init__()

        self.font = None
        try:
            ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 14)
        except:
            ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 14)


        self.set_title('amogus')

        amount = 100
        self.ver_c = VerContainer()
        self.ver_c.add_widget(RectWidget(max_height=0))
        # for i in range(amount):
        #     l = LabelWidget(self.font, random_string(), bg_color=LGREEN)
        #     self.ver_c.add_widget(l)
        #     self.ver_c.add_widget(RectWidget(BLACK, max_height=1))
        # self.ver_c.move(1, 1)

        self.scroll = ScrollWidget(pref_height=200, pref_width=200)
        self.scroll.set_widget(self.ver_c)
        self.scroll.move(10, 10)
        # self.ver_c.add_widget(self.scroll)

        self.container.add_widget(self.scroll)


    def draw(self):
        super().draw()

    def process_key(self, event: pg.event.Event):
        if event.key == pg.K_SPACE:
            self.ver_c.add_widget(LabelWidget(self.font, random_string()))
        return super().process_key(event)

    def update(self):
        self.scroll.scroll -= 2
        return super().update()

        # start = (100, 100)
        # mpos = pg.mouse.get_pos()
        # pg.draw.line(self.screen, (0, 0, 0), start, mpos, 1)

        # angle_diff = pi / 6
        # angle = atan2(-(mpos[1] - start[1]), -(mpos[0] - start[0])) - angle_diff / 2
        # print(angle)
        # line_l = 50
        # for i in range(2):
        #     end = (mpos[0] + line_l * cos(angle), mpos[1] + line_l * sin(angle))
        #     pg.draw.line(self.screen, (0, 0, 0), mpos, end, 6)
        #     angle = angle + angle_diff

TestWindow.Instance = TestWindow()

TestWindow.Instance.run()