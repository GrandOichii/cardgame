

function _CreateCard(props)
    props.cost = 3
    props.power = 1
    props.life = 4
    
    local result = CardCreation:Unit(props)

    return result
end