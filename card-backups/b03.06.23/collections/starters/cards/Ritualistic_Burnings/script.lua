
function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:Treasure('Select target Treasure for '..result.name, player.id, result.id)
            DealDamage(result.id, target.id, 3)
            DealDamageToPlayer(result.id, GetController(target.id).id, 3)
            return nil, true
        end
    )

    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:AtLeastOneTreasureInPlay()
        end
    )

    return result
end