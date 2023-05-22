
-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.power = 3
    props.life = 2

    local result = CardCreation:Unit(props)
    
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            local units = player.units
            local can = {}
            for _, unit in ipairs(units) do
                if unit.name ~= result.name then
                    can[#can+1] = unit
                end
            end
            if #can == 0 then
                return
            end
            local card = can[math.random( #can )]
            card.health = card.health + 1
        end)
        :Build()
    
    return result
end