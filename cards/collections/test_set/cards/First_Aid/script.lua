

function _CreateCard(props)
    props.cost = 1

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            GainLife(player.id, 2)
            DrawCards(player.id, 1)
            return nil, true
        end
    )

    return result
end