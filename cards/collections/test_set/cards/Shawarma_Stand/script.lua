
-- TODO not tested
function _CreateCard(props)
    props.cost = 5
    props.life = 2

    local result = CardCreation:Treasure(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Effect(function (player, args)
            local units = player.units
            for _, unit in ipairs(units) do
                unit.power = unit.power + 1
            end
        end)
        :Build()

    -- TODO should change, has to be a separate trigger for "destroyed"
    local prevLeave = result.LeavePlay
    function result:LeavePlay(player)
        prevLeave(self, player)
        local card = SummonCard('test_set', 'Angry Cook')
        RequestPlaceInUnits(card.id, player.id)
    end

    return result
end