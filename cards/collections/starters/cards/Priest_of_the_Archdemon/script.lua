

function _CreateCard( props )
    props.cost = 2
    props.power = 2
    props.life = 2

    local result = CardCreation:Unit(props)

    result:AddKeyword('evil')

    return result
end