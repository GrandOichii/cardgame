
-- TODO not tested
function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local opponent = OpponentOf(player.id)
            DealDamageToPlayer(result.id, opponent.id, 5)
            return nil, true
        end
    )

    return result
end