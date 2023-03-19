

function _CreateCard(props)
    props.attack = 0
    props.health = 2
    props.cost = 2
    local result = CardCreation:Creature(props)

    result.triggers[#result.triggers+1] = EffectCreation.TriggerBuilder:Create()
        :CheckF(function (args)
            return args.player.id == GetController(result.id).id
        end)
        :IsSilent(false)
        :On(TRIGGERS.TURN_END)
        :Zone(ZONES.IN_PLAY)
        :EffectF(function (args)
            print('TRIGGERED')
            GainLife(args.player.id, 1)
        end)
        :Build()

    return result
end