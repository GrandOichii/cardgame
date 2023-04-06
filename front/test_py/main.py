import pygame
from sprites import *
from colors import *

FPS = 30


WIDTH = 800
HEIGHT = 600

pygame.init()
all_sprites = pygame.sprite.Group()
card1 = CardSprite()
all_sprites.add(card1)

# sound
# pygame.mixer.init() 

screen = pygame.display.set_mode((WIDTH, HEIGHT))

pygame.display.set_caption('CARDGAME TEST FRONT')

clock = pygame.time.Clock()

running = True
while running:
    # correct clock
    clock.tick(FPS)
    
    # events
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

    # draw
    screen.fill(BLACK)
    
    # refresh
    pygame.display.flip()

pygame.quit()