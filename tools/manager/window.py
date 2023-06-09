from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *

import json
import tools.manager.widgets as widgets
import requests
import tools.manager.core as core

CONFIG_PATH = 'config.json'

class CEComponent:
    def __init__(self, parent_window: 'ManagerEditor'):
        self.ce_window = parent_window


class Config:
    def load(fpath: str):
        result = Config()
        result.__dict__ = json.loads(open(fpath, 'r').read())
        return result


class StartMatchInterface:
    def start_match(self, host, port):
        print('Starting in', (host, port))


SERVER_ADDR = 'http://localhost:5026/'
class ManagerEditor(QMainWindow):
    def __init__(self):
        super().__init__()

        self.match_processor: StartMatchInterface = StartMatchInterface()

        # self.fetch_cards_signal = pyqtSignal(list[core.Card])
        # self.fetch_decks_signal = pyqtSignal(list[core.Deck])

        self.thread_pool = QThreadPool()

        self.fetch_cards()
        self.fetch_decks()
        self.fetch_records()
        self.init_ui()

    # ui
    def init_ui(self):
        self.setWindowTitle('Collection Editor')

        self.init_tabs()

        self.setMinimumSize(QSize(1000, 600))

    def init_tabs(self):
        self.tabs = QTabWidget()
        self.setCentralWidget(self.tabs)

        self.matches_tab = widgets.MatchesTab(self)
        self.tabs.addTab(self.matches_tab, 'Decks')

        self.decks_tab = widgets.DecksTab(self)
        self.tabs.addTab(self.decks_tab, 'Decks')

        self.cards_tab = widgets.CardsTab(self)
        self.tabs.addTab(self.cards_tab, 'Cards')

        # self.card_templates_tab = widgets.CardTemplatesTab(self)
        # self.tabs.addTab(self.card_templates_tab, 'Card Templates')

    # data fetching
    def fetch_cards(self):
        self.card_name_index = {}
        cards = requests.get(SERVER_ADDR + 'cards').json()
        self.cards = []
        for card in cards:
            self.cards += [core.Card.from_json(card)]
        self.collections = set()
        for card in self.cards:
            self.collections.add(card.collection)
            self.card_name_index[card.collection + '::' + card.name] = card

    def fetch_decks(self):
        self.decks = []
        self.deck_index = {}
        decks_raw = requests.get(SERVER_ADDR + 'decks').json()
        for deck_raw in decks_raw:
            deck = core.Deck.from_json(deck_raw)
            self.decks += [deck]
            self.deck_index[deck.name] = deck

    def fetch_records(self):
        # match id -- record
        self.record_index: dict = {}
        
        # TODO add back
        # data = requests.get(SERVER_ADDR + 'records').json()
        # for key, item in data.items():
        #     self.record_index[key] = core.MatchPlayback.from_json(item)

    # other
    def get_card(self, name: str):
        return self.card_name_index[name]
