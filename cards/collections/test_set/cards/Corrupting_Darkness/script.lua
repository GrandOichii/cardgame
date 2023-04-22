

function _CreateCard(props)
    props.cost = 2
    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local opponent = OpponentOf(player.id)
        LoseLife(opponent.id, 2)
    end

    return result
end