function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        DrawCards(player.id, 2)
    end

    return result
end
