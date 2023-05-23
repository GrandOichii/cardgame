function _CreateCard(props)
    props.cost = 2
    props.life = 2
    props.power = 2

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            local card = SummonCard('test_set', 'Recruit')
            PlaceIntoHand(card.id, player.id)
            return nil, true
        end
    )

    return result
end