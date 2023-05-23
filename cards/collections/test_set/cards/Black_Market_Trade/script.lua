

function _CreateCard( props )
    props.cost = 2
    local result = CardCreation:Spell(props)

    -- TODO didn't work
    result:AddKeyword('virtuous')
    result:AddKeyword('evil')

    return result
end