

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)
    local prevEffect = result.Effect
    
    function result:Effect(player)
        prevEffect(self, player)
        local opponent = OpponentOf(player.id)
        LoseLife(opponent.id, 3)
        GainLife(player.id, 3)
    end

    return result
end