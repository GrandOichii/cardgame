
-- TODO not tested
function _CreateCard(props)
    props.cost = 4
    props.power = 6
    props.life = 6

    local result = CardCreation:Unit(props)
    result.mutable.summonAmount = {
        min = 6,
        current = 8,
        max = 9
    }
    
    result.OnEnterP:AddLayer(
        function (player)
            for i=1,result.mutable.summonAmount.current do
                local card = SummonCard(player.id, 'test_set', 'Throw Rock')
    
                PlaceOnTopOfDeck(player.id, card.id)
                ShuffleDeck(player.id)
            end
            return nil, true
        end
    )
    
    return result
end