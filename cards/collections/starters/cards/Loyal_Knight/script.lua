
-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.power = 2
    props.life = 2

    local result = CardCreation:Unit(props)
    
    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        local lanes = player.lanes
        for i, lane in ipairs(lanes) do
            if lane.isSet and lane.unit.id == self.id and i ~= #lanes then
                local c = SummonCard('starters', 'New Recruit')
                PlaceInUnits(c.id, player.id, i+1)
                break
            end
        end
    end
    
    return result
end