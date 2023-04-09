

function _CreateCard(props)
    props.cost = 1

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        GainLife(player.id, 2)
        DrawCards(player.id, 1)
    end
    
    return result
end