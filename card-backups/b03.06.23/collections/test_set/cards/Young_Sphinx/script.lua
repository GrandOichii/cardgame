

function _CreateCard(props)
    props.cost = 5
    props.power = 3
    props.life = 2

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            DrawCards(player.id, 2)
            return nil, true
        end
    )

    return result
end