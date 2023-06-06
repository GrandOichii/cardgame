# TODO

CARD_MANIFEST_FILE = 'manifest.json'

class Card:
    def __init__(self) -> None:
        self.name: str = ''
        self.type: str = ''
        self.text: str = ''
        self.collection: str = ''
        self.power: int = 0
        self.life: int = 0
        self.script: str = ''
        # TODO
        self.ref_cards: list = []

        self.summoned = False

    def from_json(j: dict) -> 'Card':
        result = Card()
        result.name = j['name']
        result.type = j['type']
        result.collection = j['collection']
        result.text = j['text']

        # result.power = j['power']
        # result.life = j['life']
        # TODO
        
        return result
    
class DeckCard:
    def __init__(self, name: str, amount: int):
        self.name: str = name
        self.amount: int = amount

    
class Deck:
    def __init__(self):
        self.name: str = ''
        self.bond: str = ''
        self.cards: list[DeckCard] = []

    def from_json(t: dict):
        result = Deck()
        result.name = t['name']
        result.bond = t['bond']
        for card in t['cards'].items():
            result.cards += [DeckCard(card[0], card[1])]
        return result
    
    def to_text(self):
        # TODO BAD
        result = self.bond
        for card in self.cards:
            result += f'\r\n{card.amount} {card.name}'
        return result
    

class MatchRecord:
    def __init__(self):
        # TODO
        self.id: str = ''
        self.seed: int = ''
        self.status: str = ''
        self.winner: str = ''
        self.timeStart: str = ''
        self.timeEnd: str = ''

    def from_json(d: dict):
        result = MatchRecord()
        result.__dict__ = d
        return result
    