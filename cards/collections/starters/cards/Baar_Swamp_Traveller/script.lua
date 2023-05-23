

-- TODO not tested
function _CreateCard(props)
    props.cost = 2
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)
    local rememberedLane = nil

    result.PreDeathP:AddLayer(
        function ()
            local owner = GetController(result.id)
            for i, lane in ipairs(owner.lanes) do
                if lane.isSet and lane.unit.id == result.id then
                    rememberedLane = i
                    break
                end
            end
            Log('WARN: TRIED TO REMEBER LANE POS OF '..result.name..', BUT COULD NOT FIND IT')
        end
    )

    -- TODO replace with on destroy
    result.LeavePlayP:AddLayer(
        function (player)
            local c = SummonCard('starters', 'Risen Dead')
            if rememberedLane ~= nil then
                PlaceInUnits(c.id, player.id, rememberedLane-1)
            end
        end
    )

    return result
end