

-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.power = 1
    props.life = 3

    local result = CardCreation:Unit(props)
    result.OnEnterP:AddLayer(
        function (player)
            local hand = player.hand
            for _, card in ipairs(hand) do
                card:PowerUp()
            end
            return nil, true
        end
    )

    return result
end