

function _CreateCard(props)
    props.attack = 1
    props.health = 2
    props.cost = 2
    local card = CardCreation:Unit(props)

    return card
end

