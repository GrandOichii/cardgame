

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)
    result.mutable.cardDraw = {
        min = 3,
        current = 3,
        max = 4
    }
    result.mutable.cardDiscard = {
        min = 1,
        current = 1,
        max = 2
    }

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        DrawCards(player.id, self.mutable.cardDraw.current)
        player = PlayerByID(player.id)
        Common:DiscardCards('Discard a card to '..self.name, player, self.mutable.cardDiscard.current)
    end

    return result
end