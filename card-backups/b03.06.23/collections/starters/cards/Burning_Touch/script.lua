
-- TODO untested
function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)
    result.mutable.damage = {
        min = 2,
        current = 2,
        max = 3
    }

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:Damageable('Select target for '..result.name, player.id, result.id)
            if target.hand ~= nil then
                DealDamageToPlayer(result.id, target.id, result.mutable.damage)
                return nil, true
            end
            DealDamage(result.id, target.id, result.mutable.damage)
            return nil, true
        end
    )

    return result
end