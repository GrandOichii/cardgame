
-- TODO not tested
function _CreateCard(props)
    props.cost = 2
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        DrawCards(player.id, 1)
        GainLife(player.id, 1)
    end

    return result
end