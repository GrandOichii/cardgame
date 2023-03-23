

function _CreateCard(props)
    props.cost = 3
    props.life = 2
    
    local result = CardCreation:Treasure(props)

    return result
end