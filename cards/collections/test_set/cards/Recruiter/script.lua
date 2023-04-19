function _CreateCard(props)
    props.cost = 2
    props.life = 2
    props.power = 2

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        local card = SummonCard('test_set', 'Recruit')
        PlaceIntoHand(card.id, player.id)
    end

    return result
end