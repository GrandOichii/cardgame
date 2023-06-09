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
            result += f'\n{card.amount} {card.name}'
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

        self.p1Port: str = ''
        self.p2Port: str = ''

    def from_json(d: dict):
        result = MatchRecord()
        result.__dict__ = d
        return result
    

class MatchConfig:
    def __init__(self) -> None:
        self.seed: int = 0

    def from_json(j: dict) -> 'MatchConfig':
        result = MatchConfig()
        result.seed = j['seed']
        return result


class PlayerPlayback:
    def __init__(self) -> None:
        self.deck_list: str = ''
        self.responses: list[str] = []

    def from_json(j: dict) -> 'PlayerPlayback':
        result = PlayerPlayback()
        result.deck_list = j['deckList']
        result.responses = j['responses']
        return result


class MatchPlayback:
    def __init__(self):
        self.data: PlaybackData = None
        self.card_index: dict = {}

    def from_json(j: dict) -> 'MatchPlayback':
        result = MatchPlayback()
        result.data = PlaybackData.from_json(j['mRecord'])
        result.card_index = j['cardIndex']
        return result


class PlaybackData:
    def __init__(self) -> None:
        self.players: list[PlayerPlayback] = []
        self.config: MatchConfig = None
        self.timestamp: str = ''

    def from_json(j: dict) -> 'PlaybackData':
        result = PlaybackData()
        result.timestamp = j['timestamp']
        result.config = MatchConfig.from_json(j['config'])
        for item in j['players']:
            result.players += [PlayerPlayback.from_json(item)]
        return result