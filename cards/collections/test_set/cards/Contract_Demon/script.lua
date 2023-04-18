

function _CreateCard(props)
    props.cost = 6
    props.power = 6
    props.life = 6

    local result = CardCreation:Unit(props)

    -- TODO add check that owner gained life
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common.AlwaysTrue)
        :IsSilent(false)
        :On(TRIGGERS.LIFE_GAIN)
        :Zone(ZONES.UNITS)
        :Cost(Common:NoCost())
        :Effect(function (player, args)
            LoseLife(player.id, args.amount)
            DrawCards(player.id, args.amount)
        end)
        :Build()

    return result
end