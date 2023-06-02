# TODO

CARD_MANIFEST_FILE = 'manifest.json'

class Card:
    def __init__(self) -> None:
        self.name: str = ''
        self.type: str = ''
        self.text: str = ''
        self.script: str = ''
        # TODO
        self.ref_cards: list = []

        self.summoned = False

    def load(dir: str) -> 'Card':
        # TODO
        result = Card()

        return result