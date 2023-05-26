from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *
from PyQt5.QtWidgets import QWidget

import window as ce
import core

class CEComponent:
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        self.ce_window = parent_window


class CollectionsTab(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.init_ui()

    def init_ui(self):
        self.main_layout = QHBoxLayout()
        
        self.list_layout = QVBoxLayout()

        self.list_widget = QListWidget()
        self.add_collection_button = QPushButton('Add')
        # TODO
        self.remove_collection_button = QPushButton('Remove')
        # TODO

        self.list_layout.addWidget(self.list_widget)
        self.list_layout.addWidget(self.add_collection_button)
        self.list_layout.addWidget(self.remove_collection_button)

        self.main_layout.addLayout(self.list_layout, 1)

        self.cards_widget = QWidget()
        self.cards_layout = QVBoxLayout()

        self.cards_scroll = QScrollArea()
        self.cards_scroll.setVerticalScrollBarPolicy(Qt.ScrollBarAlwaysOn)
        self.cards_scroll.setHorizontalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        # self.cards_scroll.setWidgetResizable(True)

        self.cards_buttons_layout = QHBoxLayout()
        self.cards = CardsLayout(self.ce_window)
        # TODO when adding new widgets, the thing just shrinks in size, BAD, should go under scroll
        for i in range(141):
            # self.cards.add_card(CardWidget(self.ce_window, core.Card()))
            self.cards.add_card(QLabel('amogus'))
        cards_widget = QWidget()
        cards_widget.setLayout(self.cards)
        self.cards_scroll.setWidget(cards_widget)
        
        self.add_card_button = QPushButton('Add card')
        # TODO
        self.remove_card_button = QPushButton('Remove card')
        # TODO

        self.cards_buttons_layout.addWidget(self.add_card_button)
        self.cards_buttons_layout.addWidget(self.remove_card_button)
        self.cards_layout.addWidget(self.cards_scroll)
        self.cards_layout.addLayout(self.cards_buttons_layout)
        self.cards_widget.setLayout(self.cards_layout)

        self.main_layout.addWidget(self.cards_widget, 3)
        # self.cards_widget.setEnabled(False)

        self.setLayout(self.main_layout)



class DecksTab(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.init_ui()

    def init_ui(self):
        # TODO
        ...


class CardTemplatesTab(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.init_ui()

    def init_ui(self):
        # TODO
        ...


class CardWidget(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor', card: 'core.Card'):
        # TODO card type

        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.card: core.Card = card
        self.init_ui()

    def init_ui(self):
        self.setMinimumSize(QSize(350, 450))
        self.setStyleSheet('background-color: red;')


CARDS_PER_LINE = 3
class CardsLayout(CEComponent, QGridLayout):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        CEComponent.__init__(self, parent_window)
        QGridLayout.__init__(self)

        self.row = 0
        self.column = 0

    def add_card(self, widget: CardWidget):
        self.addWidget(widget, self.row, self.column)
        self.column += 1

        if self.column != CARDS_PER_LINE: return

        self.column = 0
        self.row += 1

