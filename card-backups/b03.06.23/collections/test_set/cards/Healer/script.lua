

function _CreateCard(props)
    props.power = 0
    props.life = 2
    props.cost = 2
    local result = CardCreation:Unit(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_END)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            GainLife(player.id, 1)
        end)
        :Build()

    return result
end