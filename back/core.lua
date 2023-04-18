TRIGGERS = {
    CARD_DRAW = 'card_draw',
    LIFE_GAIN = 'life_gain',
    TURN_START = 'turn_start',
    TURN_END = 'turn_end',
    SPELL_CAST = 'spell_cast',
}

ZONES = {
    DISCARD = 'discard',
    UNITS = 'units',
    TREASURES = 'treasures',
    BOND = 'bond'
}

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

function Common:RequireField(table, fieldName)
    if table[fieldName] == nil then
        error('Table' .. Utility:TableToStr(table) .. 'does not have field ' .. fieldName)
    end
end


-- TODO not tested
function Common:NoCost()
    return function (...)
        return true
    end
end


function Common:AlwaysTrue(...)
    return true
end


function Common:AlwaysFalse( ... )
    return false
end


function Common:HasEnoughEnergy( amount )
    return function ( player, ... )
        return player.energy >= amount
    end
end


function Common:PayEnergy( amount )
    return function ( player, ... )
        PayEnergy(player.id, amount)
        return true
    end
end


function Common:EnoughCardsInHand( amount )
    return function (player, ...)
        return #player.hand >= amount
    end
end


function Common:CostDiscard( amount )
    return function (player, ...)
        -- TODO
    end
end


function Common:IsOwnersTurn(card)
    return function (player, args)
        return args.player.id == GetController(card.id).id
    end
end


function Common:TargetUnitOrTreasure(playerID)
    local args = {}
    local d = {}
    local players = GetPlayers()
    for _, p in ipairs(players) do
        for _, unit in ipairs(p.units) do
            if unit ~= nil then
                d[unit.id] = unit
                args[#args+1] = tostring(unit.id)
            end
        end
        for _, treasure in ipairs(p.treasures) do
            d[treasure.id] = treasure
            args[#args+1] = tostring(treasure.id)
        end
    end
    local uID = PromptPlayer(playerID, 'in_play', args)
    local result = d[uID]
    return result
end


function Common:TargetUnit(playerID)
    local args = {}
    local d = {}
    local players = GetPlayers()
    for _, p in ipairs(players) do
        for _, unit in ipairs(p.units) do
            if unit ~= nil then
                d[unit.id] = unit
                args[#args+1] = tostring(unit.id)
            end
        end
    end
    local uID = PromptPlayer(playerID, 'in_play', args)
    local result = d[uID]
    return result
end


function Common:TargetTreasure(playerID)
    local args = {}
    local d = {}
    local players = GetPlayers()
    for _, p in ipairs(players) do
        for _, treasure in ipairs(p.treasures) do
            d[treasure.id] = treasure
            args[#args+1] = tostring(treasure.id)
        end
    end
    local uID = PromptPlayer(playerID, 'in_play', args)
    local result = d[uID]
    return result
end


function Common:IsOwnersSpell(card)
    return function (spell, args)
        local owner = GetController(card.id)
        return args.card.type == 'Spell' and args.caster.id == owner.id
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


function Utility:TableLength(T)
    local count = 0
    for _ in pairs(T) do count = count + 1 end
    return count
  end



-- local activated = EffectCreation:ActivatedEffectBuilder()
--     :Zone('lanes')
--     :Check(Common:HasEnoughEnergy(2))
--     :Cost(Common:PayEnergy(2))
--     :Effect(function( player )
--         print('Player ' .. player.name .. ' activated effect!')
--     end)
--     :Build()

-- print(Utility:TableToStr(activated))


-- -- whenever a unit an opponent is destroyed, you may discard a card to return [CARDNAME] to your hand
-- local triggered = EffectCreation:TriggerBuilder()
--     :IsSilent(false)
--     :Zone('discard')
--     :On('destroyed')
--     :Check(Common:EnoughCardsInHand(1))
--     :Cost(Common:CostDiscard(1))
--     :Effect(function (player, args)
--         print('Player ' .. player.name .. ' has a triggered effect!')
--     end)
--     :Build()

-- print(Utility:TableToStr(triggered))


-- Card Creation
CardCreation = {}

function CardCreation:CardObject(props)
    local result = {}
    result.mutable = {} -- Things that power up the card
    
    Common:RequireField(props, 'name')
    Common:RequireField(props, 'type')
    Common:RequireField(props, 'cost')
    Log('Creating card object for card '..props.name)

    result.name = props.name
    result.type = props.type
    result.cost = props.cost
    result.triggers = {}

    function result:CanPlay(player)
        return Common:HasEnoughEnergy(self.cost)(player)
    end

    function result:PayCosts(player)
        return Common:PayEnergy(self.cost)(player)
    end

    function result:Play(player)
        Log('Player '..player.name .. ' played card ' .. self.name)
    end

    function result:PowerUp()
        Log('Powering up '..self.id)
        for key, value in pairs(self.mutable) do
            local new = value.current + 1
            if new <= value.max then
                self.mutable[key].current = new
            else
                Log('Did not power up value '..key..': '..value.current..' is the max')
            end
        end
    end

    function result:CanPowerUp()
        return Utility:TableLength(self.mutable) > 0
    end

    function result:PowerDown()
        Log('Powering up '..self.id)
        for key, value in pairs(self.mutable) do
            local new = value.current - 1
            if new >= value.min then
                self.mutable[key].current = new
            else
                Log('Did not power down value '..key..': '..value.current..' is the min')
            end
        end
    end

    function result:CanPowerDown()
        return Utility:TableLength(self.mutable) > 0
    end

    return result
end


function CardCreation:Spell(props)
    local result = CardCreation:CardObject(props)

    function result:Effect(player)
        Log('Spell effect of ' .. result.name .. ', played by ' .. player.name)
    end

    -- TODO not tested
    local prevPlay = result.Play;
    function result:Play(player)
        Emit(TRIGGERS.SPELL_CAST, {
            card = self,
            caster = player
        })
        prevPlay(self, player)
        result:Effect(player)
        PlaceIntoDiscard(self.id, player.id)
    end

    return result
end


function CardCreation:Source(props)
    props.cost = 0
    local result = CardCreation:Spell(props)

    function result:Effect(player)
        IncreaseMaxEnergy(player.id, 1)
    end

    function result:CanPlay(player)
        return player.shared.sourceCount > 0
    end

    function result:PayCosts(player)
        player.shared.sourceCount = player.shared.sourceCount - 1
        return true
    end

    return result

end


function CardCreation:InPlay(props)
    local result = CardCreation:CardObject(props)

    -- TODO not currently in use by engine
    function result:LeavePlay(player)
        -- print(Utility:TableToStr(self))
        -- print(player)
        Log('Card ' .. self.name .. ', controlled by ' .. player.name .. ', is leaving play')
    end

    function result:OnEnter(player)
        Log('Called OnEnter func of '..self.name)
    end

    return result
end


function CardCreation:Bond(props)
    props.cost = 0

    local result = CardCreation:InPlay(props)

    return result
end


function CardCreation:Damageable(props)
    Common:RequireField(props, 'life')

    local result = CardCreation:InPlay(props)
    result.life = props.life
    result.baseLife = props.life

    local prevLeave = result.LeavePlay
    function result:LeavePlay(player)
        prevLeave(self, player)
        self.life = self.baseLife
    end

    return result
end


function CardCreation:Treasure(props)
    local result = CardCreation:Damageable(props)

    local prevPlay = result.Play
    function result:Play(player)
        prevPlay(self, player)
        PlaceInTreasures(self.id, player.id)
    end

    return result
end


function CardCreation:Unit(props)
    Common:RequireField(props, 'power')

    local result = CardCreation:Damageable(props)

    result.power = props.power
    result.basePower = props.power

    local prevPlay = result.Play
    function result:Play(player)
        prevPlay(self, player)
        PlaceInUnits(self.id, player.id)
    end

    local prevLeave = result.LeavePlay
    function result:LeavePlay(player)
        prevLeave(self, player)
        self.power = self.basePower
    end

    return result
end