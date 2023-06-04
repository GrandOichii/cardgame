

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            DrawCards(player.id, 2)
            return nil, true
        end
    )

    return result
end