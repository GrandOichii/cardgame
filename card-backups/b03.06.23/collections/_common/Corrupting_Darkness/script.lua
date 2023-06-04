

function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local opponent = OpponentOf(player.id)
            LoseLife(opponent.id, 2)
            return nil, true
        end
    )

    return result
end