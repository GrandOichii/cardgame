function _CreateCard(props)
    props.cost = 2
    props.life = 2
    props.power = 2
    return CardCreation:Unit(props)
end