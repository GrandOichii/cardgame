

function _CreateCard(props)
    props.power = 1
    props.life = 2
    props.cost = 2
    local card = CardCreation:Unit(props)

    return card
end

