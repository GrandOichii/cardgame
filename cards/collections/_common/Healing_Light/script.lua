

function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            GainLife(player.id, 3)
            return nil, true
        end
    )

    return result
end