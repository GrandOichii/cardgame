
-- TODO not tested
function _CreateCard(props)
    props.cost = 2
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            DrawCards(player.id, 1)
            GainLife(player.id, 1)
            return nil, true
        end
    )

    return result
end