

-- TODO not tested
function _CreateCard(props)
    props.cost = 4
    props.power = 4
    props.life = 3

    local result = CardCreation:Unit(props)
    result:AddKeyword('evil')
    
    result.OnEnterP:AddLayer(
        function (player)
            DrawCards(player.id, 1)
            LoseLife(player.id, 2)
            return nil, true
        end
    )

    
    return result
end