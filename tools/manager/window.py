from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *

import json
import widgets

CONFIG_PATH = 'config.json'

class CEComponent:
    def __init__(self, parent_window: 'CollectionEditor'):
        self.ce_window = parent_window


class Config:
    def load(fpath: str):
        result = Config()
        result.__dict__ = json.loads(open(fpath, 'r').read())
        return result


class CollectionEditor(QMainWindow):
    def __init__(self):
        super().__init__()

        self.init_ui()
        self.load()

    # ui
    def init_ui(self):
        self.setWindowTitle('Collection Editor')

        self.init_tabs()

        self.setMinimumSize(QSize(800, 600))

    def init_tabs(self):
        self.tabs = QTabWidget()
        self.setCentralWidget(self.tabs)

        self.collections_tab = widgets.CollectionsTab(self)
        self.tabs.addTab(self.collections_tab, 'Collections')

        self.decks_tab = widgets.DecksTab(self)
        self.tabs.addTab(self.decks_tab, 'Decks')

        self.card_templates_tab = widgets.CardTemplatesTab(self)
        self.tabs.addTab(self.card_templates_tab, 'Card Templates')

    # configuration loading
    def load(self):
        try:
            config = Config.load(CONFIG_PATH)
            # TODO
        except:
            QMessageBox.warning(self, 'Configuration loading', 'Failed to load configuration file')
            # TODO doesn't close
            self.close()