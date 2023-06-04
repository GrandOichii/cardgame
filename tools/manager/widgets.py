import typing
from PyQt5 import QtCore, QtGui
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *
from PyQt5.QtWidgets import QWidget

from flow import FlowLayout
import window as ce
import core
import pyperclip

class CEComponent:
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        self.ce_window = parent_window


class BondComboBox(CEComponent, QComboBox):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        CEComponent.__init__(self, parent_window)
        QComboBox.__init__(self)

        self.populate()

    def populate(self):
        cards = self.ce_window.cards
        bonds = [f'{card.collection}::{card.name}' for card in cards if card.type == 'Bond']
        self.addItem('')
        self.addItems(bonds)
        self.setCurrentText('')


class CollectionListItem(QListWidgetItem):
    def __init__(self, parent_list):
        super().__init__()
        self.parent_list = parent_list

    def add_card_condition(self, card: core.Card):
        raise 'Not Implemented'
        

class AllCollectionListItem(CollectionListItem):
    def __init__(self, parent_list):
        super().__init__(parent_list)
        self.setText('All')

    def add_card_condition(self, card: core.Card):
        return True
    

class SpecificCollectionListItem(CollectionListItem):
    def __init__(self, parent_list, col_name: str):
        super().__init__(parent_list)

        self.col_name = col_name
        self.setText(self.col_name)
    
    def add_card_condition(self, card): 
        return card['collection'] == self.col_name


class CardsTab(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.init_ui()
    #     self.test_timer = QTimer()
    #     self.test_timer.timeout.connect(self.test_timeout_action)

    # def test_click_action(self):
    #     self.add_collection_button.setText('Add (Copied!)')
    #     self.test_timer.start(1000)

    # def test_timeout_action(self):
    #     self.add_collection_button.setText('Add')

    def init_ui(self):
        self.main_layout = QHBoxLayout()
        
        self.list_layout = QVBoxLayout()

        self.collections_list_widget = QListWidget()
        self.collections_list_widget.itemClicked.connect(self.collection_list_item_clicked_action)
        self.collections_list_widget.addItem(AllCollectionListItem(self.collections_list_widget))
        for col in self.ce_window.collections:
            self.collections_list_widget.addItem(SpecificCollectionListItem(self.collections_list_widget, col))

        self.add_collection_button = QPushButton('Add')
        # self.add_collection_button.clicked.connect(self.test_click_action)
        # TODO
        self.remove_collection_button = QPushButton('Remove')
        # TODO

        self.list_layout.addWidget(self.collections_list_widget)
        self.list_layout.addWidget(self.add_collection_button)
        self.list_layout.addWidget(self.remove_collection_button)

        self.main_layout.addLayout(self.list_layout, 1)

        self.cards_widget = QWidget()
        self.cards_layout = QVBoxLayout()

        self.search_line = QLineEdit()
        self.search_line.setPlaceholderText('Search by card name')
        self.search_line.textChanged.connect(self.search_line_text_changed_action)

        self.cards_scroll = QScrollArea()
        self.cards_scroll.setVerticalScrollBarPolicy(Qt.ScrollBarAlwaysOn)
        self.cards_scroll.setHorizontalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        self.cards_scroll.setWidgetResizable(True)

        self.cards_buttons_layout = QHBoxLayout()
        self.cards = CardsLayout(self.ce_window)
        self.cards_widget.setStyleSheet('''
        #cardWidget:hover {
            border: 1px solid red;            
        }
        ''')

        for card in self.ce_window.cards:
            w = CardWidget(self.ce_window, card)
            self.cards.add_card(w)

        cards_widget = QWidget()
        cards_widget.setLayout(self.cards)
        self.cards_scroll.setWidget(cards_widget)
        
        self.add_card_button = QPushButton('Add card')
        # TODO
        self.remove_card_button = QPushButton('Remove card')
        # TODO

        self.cards_buttons_layout.addWidget(self.add_card_button)
        self.cards_buttons_layout.addWidget(self.remove_card_button)
        
        self.cards_layout.addWidget(self.search_line)
        self.cards_layout.addWidget(self.cards_scroll)
        self.cards_layout.addLayout(self.cards_buttons_layout)
        self.cards_widget.setLayout(self.cards_layout)

        self.main_layout.addWidget(self.cards_widget, 3)
        # self.cards_widget.setEnabled(False)

        self.setLayout(self.main_layout)

    # actions
    def search_line_text_changed_action(self):
        self.cards.empty()
        # self.cards_scroll.repaint()
        t = self.search_line.text()
        for card in self.ce_window.cards:
            if t in card.name:
                w = CardWidget(self.ce_window, card)
                self.cards.add_card(w)

    def collection_list_item_clicked_action(self, item: CollectionListItem):
        # l = self.collections_list_widget
        # l.clear()
        self.cards.empty()
        for card in self.ce_window.cards:
            if item.add_card_condition(card):
                w = CardWidget(self.ce_window, core.Card.from_json(card))
                self.cards.add_card(w)


class DeckCardListItem(QListWidgetItem):
    def __init__(self, card: core.DeckCard):
        super().__init__()

        self.card = card
        self.set_widget()

    def set_widget(self):
        self.widget = QWidget()
        layout = QHBoxLayout()

        self.name_label = QLabel(self.card.name)
        self.name_label.setSizePolicy(QSizePolicy.Preferred, QSizePolicy.Fixed)
        
        self.amount_spin = QSpinBox()
        self.amount_spin.setValue(self.card.amount)
        self.amount_spin.setSizePolicy(QSizePolicy.Fixed, QSizePolicy.Fixed)

        layout.addWidget(self.name_label)
        layout.addWidget(self.amount_spin)
        self.widget.setLayout(layout)

        # self.widget = QLabel(self.card.name)
        # print(self.card.name)

        self.setSizeHint(self.widget.sizeHint())


class DeckCardWidget(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor', card: core.DeckCard):
        CEComponent.__init__(self, parent_window)
        QWidget.__init__(self)

        self.card = card

        self.init_ui()

    def init_ui(self):
        layout = QVBoxLayout()

        card = self.ce_window.card_name_index[self.card.name]
        self.card_widget = CardWidget(self.ce_window, card)
        self.amount_widget = QSpinBox()
        self.amount_widget.setValue(self.card.amount)

        layout.addWidget(self.card_widget)
        layout.addWidget(self.amount_widget)

        self.setLayout(layout)


class AddCardToDeckButton(QLabel):
    def __init__(self) -> None:
        super().__init__()
        self.setStyleSheet('''
        QLabel {
            margin: 8.5px;
            border: 1px solid black;
        }
        QLabel:hover {
            border: 1px solid red;
        }
        ''')
        # self.setObjectName('cardWidget')
        image = QPixmap('content/plus.png')
        image = image.scaledToWidth(128).scaledToHeight(128)

        self.setPixmap(image)

        self.setAlignment(Qt.AlignCenter)

        # self.setText('AMOGUS')


class DeckEditArea(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        CEComponent.__init__(self, parent_window)
        QWidget.__init__(self)

        self.text_copied_timer = QTimer()
        self.text_copied_timer.timeout.connect(self.copy_timer_timeout_action)
        self.init_ui()

    def init_ui(self):
        # TODO? could add ? button near bond to show the card in a different window
        layout = QVBoxLayout()

        bond_layout = QHBoxLayout()

        bond_label = QLabel('Bond')
        bond_label.setSizePolicy(QSizePolicy.Fixed, QSizePolicy.Preferred)

        self.bond_box = BondComboBox(self.ce_window)

        bond_layout.addWidget(bond_label)
        bond_layout.addWidget(self.bond_box)

        main_deck_label = QLabel('Main deck')

        # self.cards_list = QListWidget()
        cards_list_widget = QWidget()
        self.cards_list = FlowLayout()
        cards_list_widget.setLayout(self.cards_list)
        cards_list_scroll = QScrollArea()
        cards_list_scroll.setWidgetResizable(True)
        cards_list_scroll.setWidget(cards_list_widget)
        # TODO

        save_button = QPushButton('Save')
        # TODO

        self.copy_button = QPushButton('Copy to clipboard')
        self.copy_button.clicked.connect(self.copy_action)

        layout.addLayout(bond_layout)
        layout.addWidget(main_deck_label)
        layout.addWidget(cards_list_scroll)
        layout.addWidget(save_button)
        layout.addWidget(self.copy_button)
        self.setLayout(layout)

    def load(self, deck: core.Deck):
        self.bond_box.setCurrentText(deck.bond)
        self.cards_list.empty()

        for card in deck.cards:
            cw = DeckCardWidget(self.ce_window, card)
            self.cards_list.addWidget(cw)
        # add plus button
        add_button = AddCardToDeckButton()
        add_button.setFixedSize(cw.sizeHint())
        # TODO
        self.cards_list.addWidget(add_button)

    # action
    def copy_action(self):
        res = self.bond_box.currentText()
        for i in range(self.cards_list.count()-1):
            item: DeckCardListItem = self.cards_list._items[i]
            res += f'\n{item.amount_spin.value()} {item.name_label.text()}'
        pyperclip.copy(res)

        self.copy_button.setText('Copied!')
        self.text_copied_timer.start(2000)

    def copy_timer_timeout_action(self):
        self.copy_button.setText('Copy to clipboard')
        

class DeckListItem(QListWidgetItem):
    def __init__(self, deck: core.Deck):
        super().__init__()

        self.deck = deck
        self.setText(self.deck.name)


class DecksTab(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.init_ui()

    def init_ui(self):
        layout = QHBoxLayout()

        decks_list_layout = QVBoxLayout()
        self.decks_list = QListWidget()
        self.decks_list.itemClicked.connect(self.list_item_clicked_action)
        # populate
        for deck in self.ce_window.decks:
            li = DeckListItem(deck)
            self.decks_list.addItem(li)

        add_deck_button = QPushButton('Add')
        # TODO
        remove_deck_button = QPushButton('Delete')
        # TODO

        decks_list_layout.addWidget(self.decks_list)
        decks_list_layout.addWidget(add_deck_button)
        decks_list_layout.addWidget(remove_deck_button)
        
        layout.addLayout(decks_list_layout, 1)
        
        self.deck_edit_area = DeckEditArea(self.ce_window)
        self.deck_edit_area.setEnabled(False)
        layout.addWidget(self.deck_edit_area, 3)
        self.setLayout(layout)

        # self.deck_edit_area.load(self.ce_window.decks[0])

    def list_item_clicked_action(self, item: DeckListItem):
        self.deck_edit_area.setEnabled(True)
        self.deck_edit_area.load(item.deck)


class CardTemplatesTab(CEComponent, QWidget):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.init_ui()

    def init_ui(self):
        # TODO
        ...


class CardText(QTextEdit):
    def __init__(self, parent: 'CardWidget', text: str):
        super().__init__(text)
        self.setReadOnly(True)

        self.card_parent = parent

    def mousePressEvent(self, e: QMouseEvent) -> None:
        # pass
        return self.card_parent.mousePressEvent(e)


SIZE_SCALE = 5
class CardWidget(CEComponent, QFrame):
    def __init__(self, parent_window: 'ce.CollectionEditor', card: 'core.Card'):
        # TODO card type

        QWidget.__init__(self)
        CEComponent.__init__(self, parent_window)

        self.card: core.Card = card
        self.setObjectName('cardWidget')

        self.init_ui()

    def init_ui(self):
        self.setFixedSize(QSize(35*SIZE_SCALE, 45*SIZE_SCALE))

        layout = QVBoxLayout()
        top_layout = QVBoxLayout()
        top_layout.setAlignment(Qt.AlignTop)

        self.name_label = QLabel(self.card.name)
        top_layout.addWidget(self.name_label)
        self.type_label = QLabel(self.card.type)
        top_layout.addWidget(self.type_label)
        self.text_label = QLabel(self.card.text + '\n')
        self.text_label.setWordWrap(True)
        top_layout.addWidget(self.text_label)

        # bottom_layout = QHBoxLayout()
        # power_label = QLabel('')
        # if self.card.type == 'Unit':
        #     power_label.setText(str(self.card.power))
        # bottom_layout.addWidget(power_label, alignment=Qt.AlignLeft)
        # health_label = QLabel('')
        # if self.card.type in ['Unit', 'Treasure']:
        #     health_label.setText(str(self.card.health))
        # bottom_layout.addWidget(health_label, alignment=Qt.AlignRight)

        # layout.addLayout(top_layout)

        # self.setLayout(layout)
        self.setLayout(top_layout)
        self.setFrameStyle(QFrame.StyledPanel | QFrame.Plain)
        # self.setStyleSheet('border: 1px solid black;')

    def mousePressEvent(self, a0: QMouseEvent) -> None:
        print(self.card.name)
        # return super().mousePressEvent(a0)


class CardsLayout(CEComponent, FlowLayout):
    def __init__(self, parent_window: 'ce.CollectionEditor'):
        CEComponent.__init__(self, parent_window)
        FlowLayout.__init__(self)

    def add_card(self, card):
        self.addWidget(card)
