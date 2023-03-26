

function _CreateCard(props)
    props.cost = 3
    props.life = 2

    local result = CardCreation:Treasure(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Cost(Common:NoCost())
        :Effect(function (player, args)
            GainLife(player.id, 1)
            DealDamage(result.id, result.id, 1)
        end)
        :Build()

    return result
end