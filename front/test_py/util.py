import math
import pygame as pg

def blit_text(surface: pg.Surface, text: str, pos: tuple[int, int], font: pg.font.Font, color=(0, 0, 0)):
    words = [word.split(' ') for word in text.splitlines()]  # 2D array where each row is a list of words.
    space = font.size(' ')[0]  # The width of a space.
    max_width, _ = surface.get_size()
    x, y = pos
    for line in words:
        for word in line:
            word_surface = font.render(word, 0, color)
            word_width, word_height = word_surface.get_size()
            if x + word_width >= max_width:
                x = pos[0]  # Reset the x.
                y += word_height  # Start on new row.
            surface.blit(word_surface, (x, y))
            x += word_width + space
        x = pos[0]  # Reset the x.
        y += word_height  # Start on new row.

def draw_arrow(surface: pg.Surface, start: tuple[int, int], end: tuple[int, int], width: int=6, line_length: int=50, between: float=None):
    if between is None:
        between = math.pi / 6
    pg.draw.line(surface, (0, 0, 0), start, end, width)

    angle = math.atan2(-(end[1] - start[1]), -(end[0] - start[0])) - between / 2
    # print(angle)
    for i in range(2):
        e = (end[0] + line_length * math.cos(angle), end[1] + line_length * math.sin(angle))
        pg.draw.line(surface, (0, 0, 0), e, end, width) 
        angle = angle + between