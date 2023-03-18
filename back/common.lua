CardCreation = {}

-- Card Creation
function CardCreation:CardObject(props)
    local result = {}

    result.name = props.name
    result.type = props.type
    result.cost = props.cost

    result.can_cast = function (owner)
        return owner.energy >= result.cost
    end

    return result
end


function CardCreation:Spell(props)
    local result = CardCreation:CardObject(props)
    
    result.on_cast = function(this, owner)
        -- spend energy
        TakeEnergy(owner.id, result.cost)
        PlaceIntoDiscard(this.id, owner.id)
    end
    
    return result
end


function CardCreation:Source(props)
    local result = CardCreation:CardObject(props)

    result.cost = 0
    result.can_cast = function ()
        -- TODO
        return true
    end
    result.on_cast = function (this, owner)
        -- TODO disallow any further source cards to be played this turn
        IncreaseMaxEnergy(owner.id, 1)
        PlaceIntoDiscard(this.id, owner.id)
        
    end
            
    return result
end


function CardCreation:InPlayCard(props)
    local result = CardCreation:CardObject(props)

    result.on_cast = function(this, owner)
        PlaceIntoPlay(this.id, owner.id)
        TakeEnergy(owner.id, result.cost)
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