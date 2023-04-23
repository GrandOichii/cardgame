

-- TODO not tested
function _CreateCard(props)
    props.cost = 4
    props.power = 4
    props.life = 3

    local result = CardCreation:Unit(props)
    result:AddKeyword('evil')
    
    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        DrawCards(player.id, 1)
        LoseLife(player.id, 2)
    end
    
    return result
end