

function _CreateCard(props)
    props.cost = 1
    props.power = 2
    props.life = 2

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            for i=1,3 do
                local card = SummonCard(player.id, 'test_set', 'Throw Rock')
    
                PlaceOnTopOfDeck(player.id, card.id)
                ShuffleDeck(player.id)
            end
            return nil, true
        end
    )

    return result
end