

function _CreateCard(props)
    props.cost = 4

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            -- TODO caused exception
            local laneI = PickLane(player.id)
            local players = GetPlayers()
            for _, p in ipairs(players) do
                local lane = p.lanes[laneI+1]
                
                if lane.isSet then
                    -- TODO caused exception
                    print(result.id)
                    print(lane.unit.id)
                    DealDamage(result.id, lane.unit.id, 4)
                end
            end
            return nil, true
        end
    )

    return result
end