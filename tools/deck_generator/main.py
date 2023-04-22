import random
import os.path as path
import json
from types import SimpleNamespace

def r(p: str):
    return json.loads(open(p, 'r').read(), object_hook=lambda d: SimpleNamespace(**d))


COLLECTIONS_PATH = '../../cards'
CARDS = []
cols = r(path.join(COLLECTIONS_PATH, 'manifest.json'))

for colpath in cols.collections:
    coldata = r(path.join(COLLECTIONS_PATH, colpath, 'manifest.json'))
    for cardpath in coldata.cards:
        card = r(path.join(COLLECTIONS_PATH, colpath, cardpath, 'card.json'))
        if 'summoned' in card.__dict__ and card.summoned == True or card.type == 'Bond':
            continue
        card.__dict__['collection'] = coldata.name
        CARDS += [card]


result = 'test_set::Basic Bond\n20 test_set::Source'
for i in range(30):
    card = CARDS[random.randint(0, len(CARDS)-1)]
    result += f'\n1 {card.collection}::{card.name}'
open('../../decks/generated.deck', 'w').write(result)