

function _CreateCard(props)
    props.cost = 1
    
    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local laneCount = #player.lanes
        for i = 1, laneCount, 1 do
            local card = SummonCard('test_set', 'Invasive Critter')
            PlaceInUnits(card.id, player.id, i-1)
        end
    end

    return result
end