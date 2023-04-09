

function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)
    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)

        local lanes = player.lanes
        for _, lane in ipairs(lanes) do
            if lane.isSet then
                lane.unit.power = lane.unit.power + 1
                lane.unit.life = lane.unit.life + 1
            end
        end
    end
    return result
end