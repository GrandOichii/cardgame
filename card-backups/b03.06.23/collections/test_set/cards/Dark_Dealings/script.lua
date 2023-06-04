

function _CreateCard(props)
    local result = CardCreation:Bond(props)
    result.mutable.lifeLossPerGain = {
        current = 1,
        max = 5,
        min = 0
    }

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common.OwnerGainedLife(result))
        :IsSilent(false)
        :On(TRIGGERS.LIFE_GAIN)
        :Zone(ZONES.BOND)
        :Cost(Common:NoCost())
        :Effect(function (player, args)
            local opponent = OpponentOf(player.id)
            LoseLife(opponent.id, result.mutable.lifeLossPerGain.current)
        end)
        :Build()

    return result
end