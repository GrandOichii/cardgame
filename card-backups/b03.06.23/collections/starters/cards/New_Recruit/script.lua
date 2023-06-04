function _CreateCard(props)
    props.cost = 1
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)
    
    
    
    return result
end