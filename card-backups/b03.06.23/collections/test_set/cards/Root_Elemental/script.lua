

function _CreateCard(props)
    props.cost = 4
    props.power = 2
    props.life = 2

    local result = CardCreation:Unit(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            result.power = result.power + 1
            result.life = result.life + 1
        end)
        :Build()

    return result
end