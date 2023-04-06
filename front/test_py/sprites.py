import pygame
from colors import *

CARD_HEIGHT = 213
CARD_WIDTH = 100

class CardSprite(pygame.sprite.Sprite):
    def __init__(self):
        super().__init__()

        self.image = pygame.Surface((CARD_WIDTH, CARD_HEIGHT))
        self.image.fill(WHITE)
        # self.image.fill(self.image)
        self.rect = self.image.get_rect()