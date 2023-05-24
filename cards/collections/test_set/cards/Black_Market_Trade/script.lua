

function _CreateCard( props )
    props.cost = 2
    local result = CardCreation:Spell(props)

    result:AddKeyword('virtuous')
    -- TODO didn't work
    result:AddKeyword('evil')

    return result
end