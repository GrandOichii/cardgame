
-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.life = 5

    local result = CardCreation:Treasure(props)

    result.mutable.lifeGain = {
        min = 1,
        current = 1,
        max = 3
    }

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Effect(function (player, args)
            DealDamage(result.id, result.id, 1)
            GainLife(player.id, result.mutable.lifeGain.current)
        end)
        :Build()

    return result
end