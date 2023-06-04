

function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local unit = Common.Targeting:Unit('Select target Unit for '..result.name, player.id, result.id)
            -- TODO caused an exception
            DealDamage(result.id, unit.id, 3)
            return nil, true
        end
    )

    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:AtLeastOneUnitInPlay()
        end
    )

    return result
end