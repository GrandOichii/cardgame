

function _CreateCard(props)
    props.life = 1
    props.power = 1
    props.cost = 2

    local result = CardCreation:Unit(props)

    return result
end