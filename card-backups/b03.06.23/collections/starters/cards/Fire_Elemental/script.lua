

function _CreateCard(props)
    props.cost = 5
    props.power = 8
    props.life = 8

    local result = CardCreation:Unit(props)
    
    result:AddKeyword('fast')
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            result.health = result.health // 2

            -- TODO? should change to PowerOf
            result.power = result.power // 2
            if result.health == 0 then
                Destroy(result.id)
            end
        end)
        :Build()
    
    return result
end