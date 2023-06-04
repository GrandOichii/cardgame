

function _CreateCard(props)
    props.cost = 3
    props.power = 0
    props.life = 5

    local result = CardCreation:Unit(props)
    
    
    
    return result
end