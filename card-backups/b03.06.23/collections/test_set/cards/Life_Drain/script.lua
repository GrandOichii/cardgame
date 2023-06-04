

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local opponent = OpponentOf(player.id)
            LoseLife(opponent.id, 3)
            GainLife(player.id, 3)
            return nil, true
        end
    )

    return result
end