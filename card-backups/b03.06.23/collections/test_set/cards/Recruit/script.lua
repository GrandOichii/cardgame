

function _CreateCard(props)
    props.cost = 1
    props.life = 1
    props.power = 1
    return CardCreation:Unit(props)
end