
-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.power = 2
    props.life = 2

    local result = CardCreation:Unit(props)
    
    result.OnEnterP:AddLayer(
        function (player)
            local lanes = player.lanes
            for i, lane in ipairs(lanes) do
                if lane.isSet and lane.unit.id == result.id and i ~= #lanes then
                    local c = SummonCard('starters', 'New Recruit')
                    PlaceInUnits(c.id, player.id, i)
                    break
                end
            end
            return nil, true
        end
    )
    
    return result
end