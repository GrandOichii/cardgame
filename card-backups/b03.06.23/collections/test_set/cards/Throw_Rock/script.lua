

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:Treasure('Select target Treasure for '..result.name, player.id, result.id)
            DealDamage(result.id, target.id, 1)
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