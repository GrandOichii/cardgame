

function _CreateCard(props)
    props.cost = 5
    props.power = 6
    props.life = 6
    return CardCreation:Unit(props)
end