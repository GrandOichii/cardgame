

-- TODO not tested
function _CreateCard(props)
    props.cost = 5

    local result = CardCreation:Spell(props)

    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:AtLeastOneUnitInPlay()
        end
    )

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:Unit('Select target Unit for '..result.name, player.id, result.id)
            Destroy(target.id)
            return nil, true
        end
    )

    return result
end