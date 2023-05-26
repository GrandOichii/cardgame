

-- TODO not tested
function _CreateCard(props)
    props.cost = 4
    props.power = 4
    props.life = 5

    local result = CardCreation:Unit(props)
    result.mutable.powerstones = {
        min = 0,
        current = 3,
        max = 3
    }
    
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            result:PowerDown()
            if result.mutable.powerstones.current > 0 then
                local card = SummonCard(player.id, 'starters', 'Weakened Powerstone')
            end
        end)
        :Build()
    
    return result
end