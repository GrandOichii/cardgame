from math import cos, sin, pi, atan2
import pygame as pg
from front.test_py.frame import *

class TestWindow(Window):
    def __init__(self):
        super().__init__()

        self.set_title('amogus')

    def draw(self):
        super().draw()

        start = (100, 100)
        mpos = pg.mouse.get_pos()
        pg.draw.line(self.screen, (0, 0, 0), start, mpos, 1)

        angle_diff = pi / 6
        angle = atan2(-(mpos[1] - start[1]), -(mpos[0] - start[0])) - angle_diff / 2
        print(angle)
        line_l = 50
        for i in range(2):
            end = (mpos[0] + line_l * cos(angle), mpos[1] + line_l * sin(angle))
            pg.draw.line(self.screen, (0, 0, 0), mpos, end, 6)
            angle = angle + angle_diff

TestWindow.Instance = TestWindow()

TestWindow.Instance.run()