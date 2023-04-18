

function _CreateCard(props)
    props.cost = 4
    props.power = 0
    props.life = 6
    return CardCreation:Unit(props)
end