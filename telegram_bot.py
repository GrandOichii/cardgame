import pygame as pg
import asyncio
from front.test_py.client import ClientWindow
from telegram.main import main, set_image_capturer, ImageCapturer

WINDOW = ClientWindow('localhost', 8080)
WINDOW.set_screen_size(1200, 800)

# set_image_capturer(...)
class TestFrontImageCapturer(ImageCapturer):
    def __init__(self) -> None:
        super().__init__()
        self.save_path = 'screen.png'

    def to_image(self, state):
        WINDOW.load(state)
        WINDOW.draw()
        pg.image.save(WINDOW.screen, self.save_path)

        return self.save_path
    
set_image_capturer(TestFrontImageCapturer())

asyncio.run(main())