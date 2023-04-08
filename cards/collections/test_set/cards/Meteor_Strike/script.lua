

function _CreateCard(props)
    props.cost = 4

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)

        local laneI = PickLane(player.id)
        local players = GetPlayers()
        for _, p in ipairs(players) do
            print('l1')
            local lane = p.lanes[laneI+1]
            print('l2')
            
            if lane.isSet then
                print('l3')
                DealDamage(self.id, lane.unit.id, 4)
            end
            print('Next player')
        end
        print('Finished executing effect')
    end
    
    return result
end