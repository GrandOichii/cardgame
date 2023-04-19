

function _CreateCard(props)
    props.cost = 2
    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        GainLife(player.id, 3)
    end

    return result
end