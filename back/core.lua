-- main effect creation object
EffectCreation = {}


-- returns a builder object that builds an activated effect
function EffectCreation:ActivatedEffectBuilder()
    local result = {
        args = {}
    }
    -- zone
    -- check
    -- effect
    -- cost

    function result:Build()
        local ae = {}
        -- checkers
        if self.args.zone == nil then
            error("Can't build trigger: zone is missing")
        end
        if self.args.effectF == nil then
            error("Can't build trigger: effectF is missing")
        end
        if self.args.checkF == nil then
            error("Can't build trigger: checkF is missing")
        end
        if self.args.costF == nil then
            error("Can't build trigger: costF is missing")
        end
        ae.zone = self.args.zone
        ae.effectF = self.args.effectF
        ae.checkF = self.args.checkF
        ae.costF = self.args.costF
        return ae
    end

    function result:Zone(zone)
        self.args.zone = zone
        return self
    end

    function result:Cost(costF)
        self.args.costF = costF
        return self
    end

    function result:Effect( effectF )
        self.args.effectF = effectF
        return self
    end

    function result:Check( checkF )
        self.args.checkF = checkF
        return self
    end

    return result
end


-- returns a builder that builds a trigger
function EffectCreation:TriggerBuilder()
    local result = EffectCreation:ActivatedEffectBuilder()

    local oldBuildF = result.Build
    function result:Build()
        local t = oldBuildF(self)
        if self.args.on == nil then
            error("Can't build trigger: on is missing")
        end
        if self.args.isSilent == nil then
            error("Can't build trigger: isSilent is missing")
        end
        t.on = self.args.on
        t.isSilent = self.args.isSilent
        return t
    end

    function result:IsSilent( isSilent )
        self.args.isSilent = isSilent
        return self
    end

    function result:On( on )
        self.args.on = on
        return self
    end

    return result
end


Common = {}


function Common:AlwaysTrue(...)
    return true
end


function Common:AlwaysFalse( ... )
    return false
end


-- TODO could need to replace ... with args, don't know yet
function Common:HasEnoughEnergy( amount )
    return function ( player, ... )
        -- TODO
        return true
    end
end


function Common:PayEnergy( amount )
    return function ( player, ... )
        -- TODO 
    end
end


function Common:EnoughCardsInHand( amount )
    return function (player, ...)
        -- TODO
    end
end


function Common:CostDiscard( amount )
    return function (player, ...)
        -- TODO
    end
end


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


local activated = EffectCreation:ActivatedEffectBuilder()
    :Zone('lanes')
    :Check(Common:HasEnoughEnergy(2))
    :Cost(Common:PayEnergy(2))
    :Effect(function( player )
        print('Player ' .. player.name .. ' activated effect!')
    end)
    :Build()

print(Utility:TableToStr(activated))


-- whenever a unit an opponent is destroyed, you may discard a card to return [CARDNAME] to your hand
local triggered = EffectCreation:TriggerBuilder()
    :IsSilent(false)
    :Zone('discard')
    :On('destroyed')
    :Check(Common:EnoughCardsInHand(1))
    :Cost(Common:CostDiscard(1))
    :Effect(function (player)
        print('Player ' .. player.name .. ' has a triggered effect!')
    end)
    :Build()

print(Utility:TableToStr(triggered))