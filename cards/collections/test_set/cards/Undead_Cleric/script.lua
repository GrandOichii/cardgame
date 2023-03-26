

function _CreateCard(props)
    props.cost = 2
    props.power = 2
    props.life = 1
    local result = CardCreation:Unit(props)
    
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common.AlwaysTrue)
        :IsSilent(false)
        :On(TRIGGERS.LIFE_GAIN)
        :Zone(ZONES.DISCARD)
        :Cost(Common:NoCost())
        :Effect(function (player, args)
            RemoveFromDiscard(result.id, player.id)
            PlaceIntoHand(result.id, player.id)
        end)
        :Build()

    return result
end