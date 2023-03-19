ZONES = {
    IN_PLAY = 'in_play',
    HAND = 'hand',
    ANYWHERE = 'anywhere',
    DISCARD = 'discard',
    DECK = 'deck'
}

TRIGGERS = {
    TURN_START = 'turn_start',
    TURN_END = 'turn_end'
}

-- Extends the object function with further functionality
function ExtendFunc(o, funcName, func)
    local f = o[funcName]
    o[funcName] = function (...)
        f(...)
        func(...)
    end
end


EffectCreation = {}
EffectCreation.TriggerBuilder = {}
function EffectCreation.TriggerBuilder:Create()
    local result = {
        values = {}
    }

    function result:Zone(zoneName)
        self.values.zone = zoneName
        return self
    end

    function result:On(what)
        self.values.on = what
        return self
    end

    function result:IsSilent(isSilent)
        self.values.isSilent = isSilent
        return self
    end

    function result:CheckF(checkF)
        self.values.checkF = checkF
        return self
    end

    function result:EffectF(effectF)
        self.values.effectF = effectF
        return self
    end

    function result:Build()
        local trigger = {}
        if self.values.isSilent == nil then
            error("Can't build trigger: isSilent is missing")
        end
        if self.values.zone == nil then
            error("Can't build trigger: zone is missing")
        end
        if self.values.on == nil then
            error("Can't build trigger: on is missing")
        end
        if self.values.effectF == nil then
            error("Can't build trigger: effectF is missing")
        end

        trigger.zone = self.values.zone
        trigger.on = self.values.on
        trigger.effect = self.values.effectF
        trigger.isSilent = self.values.isSilent
        trigger.check = self.values.checkF
        return trigger
    end

    return result
end
-- Effect creator
-- EffectCreation = {}
-- EffectCreation.Triggers = {}



-- function EffectCreation.Triggers:CreateZone()
--     local result = {}
--     function result:Add(isSilent, checkF, effectF)
--         result.isSilent = isSilent
--         result.triggers[#result.triggers+1] = {
--             isSilent = isSilent,
--             check = checkF,
--             effect = effectF
--         }
--     end
--     return result
-- end


-- function EffectCreation.Triggers:CreateTriggers()
--     local result = {}
--     function result:Zone(zoneName)
--         if self[zoneName] == nil then
--             local zone = EffectCreation.Triggers:CreateZone()
--             result[zoneName] = zone
            
--         end
--         return self[zoneName]
    
--     end
--     return result
-- end


-- function EffectCreation.Triggers:Create()
--     local result = {}
--     function result:On(what)
--         if self[what] == nil then
--             -- local triggers = {}
--             local triggers = EffectCreation.Triggers:CreateTriggers()
--             self[what] = triggers
--         end
--         return self[what]
--     end


--     return result
-- end


-- Card Creation
CardCreation = {}


-- Creates a card object
-- 
-- Has a can_cast function, which returns true if it's owner can cast it
--
-- Has a cast_cost function, which is called first when the player is casting the card
--
-- Creates a dummy function on_cast - spell's effect when casting it
--
-- Creates a triggers list
--
-- Required props: name, type, cost
function CardCreation:CardObject(props)
    local result = {}

    result.name = props.name
    result.type = props.type
    result.cost = props.cost
    
    result.triggers = {}


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