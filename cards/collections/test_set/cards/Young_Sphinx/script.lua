

function _CreateCard(props)
    props.cost = 5
    props.power = 3
    props.life = 2

    local result = CardCreation:Unit(props)

    -- TODO move to a different trigger?
    local prevPlay = result.Play
    function result:Play(player)
        prevPlay(self, player)
        DrawCards(player.id, 2)
    end

    return result
end