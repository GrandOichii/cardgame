Common = {}

function Common:CardObject(props)
    local result = {}

    result.name = props.name
    result.type = props.type
    result.cost = props.cost

    result.placeable = false

    return result
end


function Common:Spell(props)
    local result = Common:CardObject(props)

    return result
end


function Common:Source(props)
    local result = Common:CardObject(props)

    return result
end


function Common:InPlayCard(props)
    local result = Common:CardObject(props)

    result.placeable = true

    return result
end