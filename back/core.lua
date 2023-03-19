-- Card Creation
CardCreation = {}

-- Extends the object function with further functionality
function ExtendFunc(o, funcName, func)
    local f = o[funcName]
    o[funcName] = function (...)
        f(...)
        func(...)
    end
end


-- Creates a card object
-- 
-- Has a can_cast function, which returns true if it's owner can cast it
--
-- Has a cast_cost function, which is called first when the player is casting the card
--
-- Creates a dummy function on_cast - spell's effect when casting it
--
-- Required props: name, type, cost
function CardCreation:CardObject(props)
    local result = {}

    result.name = props.name
    result.type = props.type
    result.cost = props.cost

    result.can_cast = function (owner)
        return owner.energy >= result.cost
    end

    result.cast_cost = function(this, owner)
        -- spend energy
        TakeEnergy(owner.id, result.cost)
    end

    result.on_cast = function (this, owner)
        
    end

    return result
end


-- Creates a spell card object - after casting is placed into the graveyard
-- 
-- Extends the function cast_cost by placing the card into discard
function CardCreation:Spell(props)
    local result = CardCreation:CardObject(props)

    ExtendFunc(result, 'cast_cost', function (this, owner)
        PlaceIntoDiscard(this.id, owner.id)
    end)
    
    return result
end


-- Creates a Source card - is used to gain energy
--
-- Overrides the cost in props to cost 0
--
-- Overrides the can_cast function TODO
--
-- Overrides the on_cast
function CardCreation:Source(props)
    local result = CardCreation:Spell(props)

    result.cost = 0

    result.can_cast = function ()
        -- TODO
        return true
    end

    result.on_cast = function (this, owner)
        -- TODO disallow any further source cards to be played this turn
        IncreaseMaxEnergy(owner.id, 1)
    end
            
    return result
end


-- Creates a in play card object
--
-- Overrides the on_cast function to put the card into play
function CardCreation:InPlayCard(props)
    local result = CardCreation:CardObject(props)

    result.on_cast = function(this, owner)
        PlaceIntoPlay(this.id, owner.id)
    end

    return result
end


-- Creates a damageable card object
--
-- Required props: health
function CardCreation:Damageable(props)
    local result = CardCreation:InPlayCard(props)
    result.health = props.health
    return result
end


-- Creates a creature card object
--
-- Required props: attack
function CardCreation:Creature(props)
    local result = CardCreation:Damageable(props)
    result.attack = props.attack
    return result
end


-- Utility Functions
Utility = {}

-- Parses a table into a string
--
-- Keys are randomized
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