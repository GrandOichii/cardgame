
function _CreateCard(props)
    props.cost = 2
    props.power = 0
    props.life = 4

    local result = CardCreation:Unit(props)

    result.mutable.damagePerSpell = {
        current = 1,
        min = 0,
        max = 2
    }

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersSpell(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.SPELL_CAST)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            DealDamageToPlayer(result.id, player.id, result.mutable.damagePerSpell)
        end)
        :Build()

    return result
end