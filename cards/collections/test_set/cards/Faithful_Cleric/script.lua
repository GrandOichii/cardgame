

function _CreateCard(props)
    props.cost = 1
    props.life = 2
    props.power = 1

    local result = CardCreation:Unit(props)

    result:AddKeyword('virtuous')

    return result
end