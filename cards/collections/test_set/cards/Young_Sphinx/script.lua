

function _CreateCard(props)
    props.cost = 5
    props.power = 3
    props.life = 2

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        DrawCards(player.id, 2)
    end

    return result
end