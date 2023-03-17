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

    result.on_cast = function(this, owner)
        PlaceIntoPlay(this.id, owner.id)
    end

    return result
end


function CardCreation:Damageable(props)
    local result = CardCreation:InPlayCard(props)
    result.health = props.health
    return result
end


function CardCreation:Creature(props)
    local result = CardCreation:Damageable(props)
    result.attack = props.attack
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