import json
import select
from types import SimpleNamespace
import pygame as pg
import socket

import sprites
import colors

def test_state():
    return parse_state(open('../state_test.json', 'r').read())


def parse_state(textj):
    return json.loads(textj, object_hook=lambda d: SimpleNamespace(**d))

FPS = 30

WIDTH = 1200
HEIGHT = WIDTH * 60 / 100

HOST = 'localhost'
PORT = 8080

class Client:
    INSTANCE: 'Client'

    def __init__(self):
        self.lanes_board = None
        self.last_state = None
        
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        pg.init()

        self.screen = pg.display.set_mode((WIDTH, HEIGHT))
        # self.screen = pg.display.set_mode((0, 0), pg.FULLSCREEN)

        # global HEIGHT, WIDTH
        # WIDTH, HEIGHT = self.screen.get_size()

        hand_h = sprites.CARD_HEIGHT + 6
        pbh = (HEIGHT-hand_h) / 2
        self.first_player = sprites.PlayerBoard(WIDTH, pbh)
        self.second_player = sprites.PlayerBoard(WIDTH, pbh)
        self.hand = sprites.HandBoard(WIDTH, hand_h)
        self.second_player.set_pos((0, pbh-2))
        self.hand.set_pos((0, pbh * 2))

        # self.load(test_state())

        # sound
        # pg.mixer.init() 

        pg.display.set_caption('CARDGAME TEST FRONT')

        self.clock = pg.time.Clock()
        self.clicked = False


    def load(self, state):
        self.last_state = state

        self.first_player.load(state.players[0])
        self.second_player.load(state.players[1])

        self.hand.load(state.myData.hand)

        # card1 = CardSprite(state.players[0].bond)
        # self.all_sprites.add(card1)

    
    def configure_lanes(self, lane_count):
        # self.lanes_board = sprites.LanesBoard(lane_count, )
        self.first_player.create_lanes(lane_count)
        self.second_player.create_lanes(lane_count)


    def read_msg(self):
        # ready = select.select([self.sock], [], [], .1)
        # if not ready[0]: return ''
        message = ''
        try :
            message_length_bytes = self.sock.recv(4)
            print(message_length_bytes)
            message_length = int.from_bytes(message_length_bytes, byteorder='little')
            print(f'Message length: {message_length}')

            # Receive the message itself
            message_bytes = self.sock.recv(message_length)
            message = message_bytes.decode('utf-8')
        except socket.timeout:
            message = ''

        return message


    def send_msg(self, msg: str):
        message_length = len(msg)
        message_length_bytes = message_length.to_bytes(4, byteorder='little')
        message_bytes = msg.encode('utf-8')
        message_with_length = message_length_bytes + message_bytes
        self.sock.sendall(message_with_length)


    def start(self):
        self.sock.connect((HOST, PORT))
        # read server config
        sconfig = parse_state(self.read_msg())
        print(sconfig)
        self.configure_lanes(sconfig.lane_count)
        self.sock.settimeout(.1)
        # self.sock.setblocking(0)

        self.running = True
        while self.running:
            # correct clock
            self.clock.tick(FPS)

            # check for new data
            statej = self.read_msg()
            if statej != '':
                parsed = parse_state(statej)
                print(f'Request: {parsed.request}')
                self.load(parsed)
            
            # events
            for event in pg.event.get():
                if event.type == pg.QUIT:
                    self.running = False
                if event.type == pg.MOUSEBUTTONDOWN:
                    self.clicked = True
                if event.type == pg.KEYDOWN and event.key == pg.K_SPACE:
                    self.send_msg('pass')

            # update sprites
            # self.all_sprites.update()

            # draw
            self.screen.fill(colors.WHITE)
            if self.last_state is not None:
                self.first_player.player_draw(self.screen, self.last_state.curPlayerI == 0)
                self.second_player.player_draw(self.screen, self.last_state.curPlayerI == 1)
            self.hand.draw(self.screen)
            # self.all_sprites.draw(self.screen)
            
            self.clicked = False
            # refresh
            pg.display.flip()

        pg.quit()
        self.sock.close()
