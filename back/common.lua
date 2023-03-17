CardCreation = {}

-- Card Creation
function CardCreation:CardObject(props)
    local result = {}

    result.name = props.name
    result.type = props.type
    result.cost = props.cost

    return result
end


function CardCreation:Spell(props)
    local result = CardCreation:CardObject(props)
    
    result.on_cast = function(match, owner)
        print('Spell is placed into discard')
    end
    
    return result
end


function CardCreation:Source(props)
    local result = CardCreation:CardObject(props)

    result.cost = -1
    result.on_cast = function (this, owner)
        PlaceIntoDiscard(this.id, owner.id)
    end
            
    return result
end


function CardCreation:InPlayCard(props)
    local result = CardCreation:CardObject(props)
    
    result.on_cast = function(owner)
        print('Spell is placed into play')
    end

    return result
end

-- Utility Functions
Utility = {}

function Utility:TableToStr(t)
    if type(t) == 'table' then
        local s = '{ '
        for k,v in pairs(t) do
            if type(k) ~= 'number' then k = '"'..k..'"' end
            s = s .. '['..k..'] = ' .. Utility:TableToStr(v) .. ','
        end
        return s .. '} '
    else
        return tostring(t)
    end
end