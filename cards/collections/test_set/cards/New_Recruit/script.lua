

function _CreateCard(props)
    props.life = 1
    props.attack = 1

    local result = CardCreation:Unit(props)

    return result
end