

function _CreateCard(props)
    props.cost = 1
    props.power = 2
    props.life = 2

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        
        for i=1,3 do
            local card = SummonCard('test_set', 'Throw Rock')

            PlaceOnTopOfDeck(player.id, card.id)
            ShuffleDeck(player.id)
        end
    end

    return result
end