

function _CreateCard(props)
    props.cost = 1
    props.power = 2
    props.life = 3

    local result = CardCreation:Unit(props)

    result.mutable.perTurnLifeGain = {
        current = 0,
        min = 0,
        max = 3
    }

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            result:PowerUp()
            GainLife(player.id, result.mutable.perTurnLifeGain.current)
        end)
        :Build()

    return result
end