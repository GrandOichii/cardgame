

function _CreateCard(props)
    props.cost = 4

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)

        -- TODO caused exception
        local laneI = PickLane(player.id)
        local players = GetPlayers()
        for _, p in ipairs(players) do
            local lane = p.lanes[laneI+1]
            
            if lane.isSet then
                DealDamage(self.id, lane.unit.id, 4)
            end
        end
    end
    
    return result
end