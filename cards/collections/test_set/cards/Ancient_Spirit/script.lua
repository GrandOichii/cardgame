

function _CreateCard(props)
    props.cost = 1
    props.power = 5
    props.life = 4

    local result = CardCreation:Unit(props)
    
    result:AddKeyword('fast')
    result:AddKeyword('idu')
    
    return result
end