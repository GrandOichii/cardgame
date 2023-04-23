

-- TODO not tested
function _CreateCard(props)
    props.cost = 2
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)
    local rememberedLane = nil

    local prevPreDeath = result.PreDeath
    function result:PreDeath()
        prevPreDeath(self)
        local owner = GetController(self.id)
        for i, lane in ipairs(owner.lanes) do
            if lane.isSet and lane.unit.id == self.id then
                rememberedLane = i
                break
            end
        end
        Log('WARN: TRIED TO REMEBER LANE POS OF '..self.name..', BUT COULD NOT FIND IT')
    end

    local prevLeave = result.LeavePlay
    function result:LeavePlay(player)
        prevLeave(self, player)
        local c = SummonCard('starters', 'Risen Dead')
        if rememberedLane ~= nil then
            PlaceInUnits(c.id, player.id, rememberedLane-1)
        end
    end

    return result
end