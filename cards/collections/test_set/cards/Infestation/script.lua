

function _CreateCard(props)
    props.cost = 5

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local laneCount = #player.lanes
            for i = 1, laneCount, 1 do
                local card = SummonCard('test_set', 'Invasive Critter')
                PlaceInUnits(card.id, player.id, i-1)
            end
            return nil, true
        end
    )

    return result
end