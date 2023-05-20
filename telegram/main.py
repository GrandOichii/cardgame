
# from front.test_py.client import ClientWindow
# from ..
from types import SimpleNamespace
import socket
import json
import telebot
import telebot.types as types
import asyncio

CHAT_ID = None

# TOKEN = open('telegram/TOKEN').read()
TOKEN = open('TOKEN').read()

BOT = telebot.TeleBot(TOKEN)

HOST = 'localhost'
PORT = 8080

SOCK = socket.socket(socket.AF_INET, socket.SOCK_STREAM)


def parse(text: str):
    return json.loads(text, object_hook=lambda d: SimpleNamespace(**d))


class ImageCapturer:
    def __init__(self) -> None:
        pass

    def to_image(self, state):
        return None
    

IMAGE_CAPTURER = ImageCapturer()


def set_image_capturer(capturer: ImageCapturer):
    global IMAGE_CAPTURER
    IMAGE_CAPTURER = capturer


class MatchState:
    def __init__(self, id: str) -> None:
        self.id = id
        self.players: list[int] = []


    def run(self):
        for player_id in self.players:
            BOT.send_message(player_id, f'Match {self.id} started!')
            sock = PLAYER_SOCK_MAP[player_id]
            sock.settimeout(.1)

            config_j = read_msg(sock)
            
            # TODO remove, sending match config to players
            BOT.send_message(player_id, config_j)

        self.running = True
        while self.running:
            for player_id in self.players:
                msg = read_msg(PLAYER_SOCK_MAP[player_id])
                if msg == '':
                    continue
                # send the message
                send_state_to_user(player_id, msg)



def send_state_to_user(chatID, state_j):
    state = parse(state_j)
    keyboard = types.InlineKeyboardMarkup()

    if state.request == 'enter command':
        me = state.players[state.myData.playerI]
        attack_row = keyboard.row()
        for i, unit in enumerate(me.units):
            if not unit or unit.attacksLeft == 0: continue

            card = unit.card
            b = types.InlineKeyboardButton(text=f'Lane {i+1}: Attack with {card.name}', callback_data=f'/c attack {i}')
            attack_row.add(b)


        play_row = keyboard.row()
        for card in state.myData.hand:
            b = types.InlineKeyboardButton(text=card.name, callback_data=f'/c play {card.id}')
            play_row.add(b)

        b = types.InlineKeyboardButton(text='Pass turn', callback_data=f'/c pass')
        keyboard.add(b)

    print(state.request)
    if state.request == 'pick lane':
        print(state.players[0].units)
        for i, unit in enumerate(state.players[state.myData.playerI].units):
            b_text = f'Lane {i+1}'
            if unit is not None:
                b_text += f' ({unit.card.name})'
            b = types.InlineKeyboardButton(text=b_text, callback_data=f'/c {i}')
            keyboard.add(b)
    
    image_path = IMAGE_CAPTURER.to_image(state)
    if image_path is None:
        BOT.send_message(chatID, state_j, reply_markup=keyboard)
        return
    BOT.send_photo(chatID, open(image_path, 'rb'), reply_markup=keyboard)

@BOT.message_handler(commands=['start'])
def start(message: types.Message):
    args = message.text.split()
    if len(args) == 2:
        connect_to_match(message.chat.id, f'connect #{args[1]}', message.from_user.username)

        return
        
    BOT.send_message(message.chat.id, f'Hello, {message.from_user.username}')

    # match config
    # m_config = read_msg()
    # BOT.send_message(message.chat.id, m_config)

    # send_state(message.chat.id)


@BOT.message_handler(commands=['c'], )
def parse_command(message: types.Message):
    pid = message.chat.id
    if not pid in PLAYER_SOCK_MAP:
        return
    sock = PLAYER_SOCK_MAP[pid]
    send_msg(sock, ' '.join(message.text.split()[1:]))


@BOT.callback_query_handler(lambda call: True)
def callback(call):
    call.message.text = call.data
    parse_command(call.message)


PLAYER_SOCK_MAP: dict[int, socket.socket] = {}

MATCH_MAP: dict[str, MatchState] = {}

def check_msg(chatID, o, failed_to):
    if o == 'fail':
        BOT.send_message(chatID, f'Failed to {failed_to}, server response: {o.payload}')
        return False
    return True


# MATCH POOL MANAGEMENT
@BOT.message_handler(commands=['listmatches'])
def list_matches(message: types.Message):
    command = {
        'header': 'command',
        'payload': 'list'
    }
    send_msg(SOCK, json.dumps(command))
    m = read_msg(SOCK)
    o = json.loads(m)
    states = [
        'WaitingForPlayer',
        'InProgress',
        'Ended'
    ]
    o = dict(sorted(o.items(), key=lambda item: states.index(item[1])))
    result_msg = ''
    for pair in o.items():
        result_msg += f'{pair[0]}: {pair[1]}\n'
    if result_msg == '':
        BOT.send_message(message.chat.id, 'No available matches. Type command /newmatch to create a new match.')
        return
    BOT.send_message(message.chat.id, result_msg)


@BOT.message_handler(commands=['newmatch', 'creatematch', 'nm'])
def new_match(message: types.Message):
    command = {
        'header': 'command',
        'payload': 'new'
    }
    BOT.send_message(message.chat.id, f'Creating a new match...')
    send_msg(SOCK, json.dumps(command))
    m = read_msg(SOCK)
    o = parse(m)
    if not check_msg(message.chat.id, o, 'create a new match'):
        return
    m_id = o.payload
    BOT.send_message(message.chat.id, f'Created a new match with id {m_id}')
    BOT.send_message(message.chat.id, f'Send this link to a friend or wait for someone to join your match')
    link = f'https://t.me/cardgame_test_bot?start={m_id[1:]}'
    # BOT.send_message(message.chat.id, f'<a href="{link}">{link}</a>', parse_mode='html')
    BOT.send_message(message.chat.id, link)

    # TODO add required argument for opponent type (bot, player)

    connect_to_match(message.chat.id, f'connect {m_id}', message.from_user.username)


@BOT.message_handler(commands=['connect'])
def connect(message: types.Message):
    connect_to_match(message.chat.id, message.text, message.from_user.username)
    

def connect_to_match(chatID, message, username):
    args = message.split()
    if len(args) != 2:
        BOT.send_message(chatID, 'Incorrect number of arguments for /connect command: specify the ID of the game you want to join')
        return
    m_id = args[1]

    command = {
        'header': 'command',
        'payload': f'connect {m_id} {username}'
    }
    send_msg(SOCK, json.dumps(command))

    m = read_msg(SOCK)
    o = parse(m)
    if not check_msg(chatID, o, 'connect to match'):
        return

    BOT.send_message(chatID, 'Connected!')
    p_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    p_sock.connect((HOST, PORT))
    PLAYER_SOCK_MAP[chatID] = p_sock
    
    m = read_msg(SOCK)
    o = parse(m)

    if not m_id in MATCH_MAP:
        MATCH_MAP[m_id] = MatchState(m_id)
    MATCH_MAP[m_id].players += [chatID]

    if o.header == 'match_pending':
        return
    
    # match started
    g_match = MATCH_MAP[m_id]
    asyncio.create_task(g_match.run())

def read_msg(sock):
    # ready = select.select([sock], [], [], .1)
    # if not ready[0]: return ''
    message = ''
    try :
        message_length_bytes = sock.recv(4)
        message_length = int.from_bytes(message_length_bytes, byteorder='little')

        # Receive the message itself
        while len(message) < message_length:
            message_bytes = sock.recv(message_length)
            message += message_bytes.decode('utf-8')

    except socket.timeout:
        message = ''

    return message


def send_msg(sock: socket.socket, msg: str):
    message_length = len(msg)
    message_length_bytes = message_length.to_bytes(4, byteorder='little')
    message_bytes = msg.encode('utf-8')
    message_with_length = message_length_bytes + message_bytes
    sock.sendall(message_with_length)


def main():

    SOCK.connect((HOST, PORT))

    print('Started polling')
    BOT.polling(non_stop=True)


# def start_server():

#     while True:
#         message = read_msg(sock)
#         if message == '':
#             return
#         print(message)


# t = threading.Thread(target=start_bot)
# t.start()

# print('Started thread')
# start_server()

# t.join()