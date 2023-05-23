

function _CreateCard(props)
    -- props.cost = 1
    props.cost = 6
    props.power = 3
    props.life = 5

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            local treasures = player.treasures
            for _, card in ipairs(treasures) do
                if card:CanPowerUp() then
                    card:PowerUp()
                end
            end
            return nil, true
        end
    )
    return result
end