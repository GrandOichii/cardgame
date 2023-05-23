

-- TODO not tested
function _CreateCard(props)
    props.cost = 1

    local result = CardCreation:Spell(props)
    result.mutable.spellDamage = {
        min = 1,
        current = 1,
        max = 2
    }

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:UnitOrTreasure('Select target for '..result.name, player.id, result.id)
            DealDamage(result.id, target.id, result.mutable.spellDamage.current)
            if Common:IsInPlay(target.id) then
                return
            end
            DrawCards(player.id, 1)
            return nil, true
        end
    )

    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:AtLeastOneUnitInPlay() or Common:AtLeastOneTreasureInPlay()
        end
    )

    return result
end