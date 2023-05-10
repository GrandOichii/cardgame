from frame import *

CARD_SIZE_RATIO = 7 / 5
CARD_WIDTH = 100
CARD_HEIGHT = CARD_SIZE_RATIO * CARD_WIDTH


class CardWidget(RectWidget):
    def __init__(self):
        super().__init__(RED, CARD_WIDTH, CARD_HEIGHT, CARD_WIDTH, CARD_HEIGHT, CARD_WIDTH, CARD_HEIGHT)

    def from_card(card):
        return CardWidget()