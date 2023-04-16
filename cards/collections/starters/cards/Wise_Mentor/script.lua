

-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.power = 1
    props.life = 3

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        local hand = player.hand
        for _, card in ipairs(hand) do
            card:PowerUp()
        end
    end


    return result
end