

function _CreateCard(props)
    -- props.cost = 1
    props.cost = 6
    props.power = 3
    props.life = 5

    local result = CardCreation:Unit(props)
    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, prevOnEnter)
        local treasures = player.treasures
        for _, card in ipairs(treasures) do
            if card:CanPowerUp() then
                card:PowerUp()
            end
        end
    end
    return result
end