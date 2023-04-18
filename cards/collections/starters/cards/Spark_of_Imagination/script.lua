

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)
    result.mutable.cardDraw = {
        current = 2,
        min = 1,
        max = 4
    }

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)

        DrawCards(player.id, result.mutable.cardDraw.current)
    end
    return result
end