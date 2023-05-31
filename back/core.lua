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


function Common:FilterInPlay(player, delegate)
    local cards = {player.bond, table.unpack(player.units), table.unpack(player.treasures)}
    local result = {}
    for _, card in ipairs(cards) do
        if delegate(card) then
            result[#result+1] = card
        end
    end
    return result
end


function Common:RequireField(table, fieldName)
    if table[fieldName] == nil then
        error('Table' .. Utility:TableToStr(table) .. 'does not have field ' .. fieldName)
    end
end


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


function Common:DrawOwnerIsCardOwner( card )
    return function( player, args )
        local owner = GetController(card.id)
        return owner.id == args.player.id
    end
end


function Common:DrawOwnerIsNotCardOwner( card )
    return function( player, args )
        local owner = GetController(card.id)
        return owner.id ~= args.player.id
    end
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


function Common:PowerUpCardInPlay()
    local players = GetPlayers()
    for _, p in ipairs(players) do
        local cards = {table.unpack(p.units), table.unpack(p.treasures), p.bond}
        for _, card in ipairs(cards) do
            if card:CanPowerUp() then
                return true
            end
        end
    end
    return false
end


function Common:DiscardCards(prompt, player, amount)
    local count = 0
    for i = 1, amount do
        if #player.hand == 0 then
            break
        end
        local target = Common.Targeting:CardInHand(prompt..' (remaining: '..tostring(amount-i+1)..')', player, Common.Targeting.Selectors:All())
        RemoveFromHand(target.id, player.id)
        PlaceIntoDiscard(target.id, player.id)

        player = PlayerByID(player.id)
        count = count + 1
    end
    return count
end


Common.Targeting = {
    Selectors = {}
}


function Common.Targeting.Selectors:Filter(delegate)
    return function (cards)
        local result = {}
        for _,card in ipairs(cards) do
            if card ~= nil and delegate(card) then
                result[#result+1] = card
            end
        end
        return result
    end
end


function Common.Targeting.Selectors:All()
    return Common.Targeting.Selectors:Filter(function( card ) return true end)
end


function Common.Targeting.Selectors:AllOfPlayers( playerID )
    return Common.Targeting.Selectors:Filter(function( card ) return GetController(card.id) == playerID end)
end


function Common.Targeting:Target(prompt, playerID, configs, sourceID)
    --[[ Common:Target(player.id, {
        {
            what = 'treasure',
            which = Common:Targeting:Selectors:All()
        },
        {
            what = 'bond',
            which = Common:Targeting:Selectors:AllOfPlayers(player.id)
        },
        {
            what = 'unit',
            which = Common:Targeting:Selectors:Filter(function unit unit.power > 2 end)
        }
    })
    --]]
    local args = {}
    local d = {}
    local players = GetPlayers()

    for _, player in ipairs(players) do
        local zoneMap = {
            treasures = player.treasures,
            units = player.units,
            bond = {},
        }
        zoneMap.bond[#zoneMap.bond+1] = player.bond

        for _, config in ipairs(configs) do
            local zone = zoneMap[config.what]
            local add = config.which(zone)
            for _, card in ipairs(add) do
                d[card.id] = card
                args[#args+1] = tostring(card.id)
            end
        end
    end

    local uID = PromptPlayer(playerID, prompt, 'target', args, sourceID)
    local result = d[uID]
    return result
end


function Common.Targeting:Unit(prompt, playerID, sourceID)
    return Common.Targeting:Target(prompt, playerID, {
        {
            what = 'units',
            which = Common.Targeting.Selectors:All()
        }
    }, sourceID)
    -- local args = {}
    -- local d = {}
    -- local players = GetPlayers()
    -- for _, p in ipairs(players) do
    --     for _, unit in ipairs(p.units) do
    --         if unit ~= nil then
    --             d[unit.id] = unit
    --             args[#args+1] = tostring(unit.id)
    --         end
    --     end
    -- end
    -- local uID = PromptPlayer(playerID, 'target', args)
    -- local result = d[uID]
    -- return result
end


function Common.Targeting:Treasure(prompt, playerID, sourceID)
    return Common.Targeting:Target(prompt, playerID, {
        {
            what = 'treasures',
            which = Common.Targeting.Selectors:All()
        }
    }, sourceID)
    -- local args = {}
    -- local d = {}
    -- local players = GetPlayers()
    -- for _, p in ipairs(players) do
    --     for _, treasure in ipairs(p.treasures) do
    --         d[treasure.id] = treasure
    --         args[#args+1] = tostring(treasure.id)
    --     end
    -- end
    -- local uID = PromptPlayer(playerID, 'target', args)
    -- local result = d[uID]
    -- return result
end


function Common.Targeting:UnitOrTreasure(prompt, playerID, sourceID)
    return Common.Targeting:Target(prompt, playerID, {
        {
            what = 'treasures',
            which = Common.Targeting.Selectors:All()
        },
        {
            what = 'units',
            which = Common.Targeting.Selectors:All()
        }
    }, sourceID)
    -- local args = {}
    -- local d = {}
    -- local players = GetPlayers()
    -- for _, p in ipairs(players) do
    --     for _, unit in ipairs(p.units) do
    --         if unit ~= nil then
    --             d[unit.id] = unit
    --             args[#args+1] = tostring(unit.id)
    --         end
    --     end
    --     for _, treasure in ipairs(p.treasures) do
    --         d[treasure.id] = treasure
    --         args[#args+1] = tostring(treasure.id)
    --     end
    -- end
    -- local uID = PromptPlayer(playerID, 'target', args)
    -- local result = d[uID]
    -- return result
end


function Common.Targeting:CardInHand(prompt, player, delegate)
    local args = {}
    local d = {}
    for _, card in ipairs(player.hand) do
        if delegate(card) then
            d[card.id] = card
            args[#args+1] = tostring(card.id)
        end
    end
    local uID = PromptPlayer(player.id, prompt, 'in_hand', args)
    local result = d[uID]
    return result
end


function Common:IsOwnersSpell(card)
    return function (spell, args)
        local owner = GetController(card.id)
        return args.card.type == 'Spell' and args.caster.id == owner.id
    end
end


function Common:AtLeastOneUnitInPlay()
    local players = GetPlayers()
    for _, player in ipairs(players) do
        for _, unit in ipairs(player.units) do
            if unit ~= nil then
                return true
            end
        end
    end
    return false
end


function Common:AtLeastOneTreasureInPlay()
    local players = GetPlayers()
    for _, player in ipairs(players) do
        for _, unit in ipairs(player.treasures) do
            if unit ~= nil then
                return true
            end
        end
    end
    return false
end


function Common:HasCardsInHand(cardName, player, amount)
    amount = amount or 1

    local count = 0
    for _, card in ipairs(player.hand) do
        if card.name == cardName then
            count = count + 1
        end
    end
    return count >= amount
end


function Common:OwnerGainedLife(card)
    return function (player, args)
        return player.id == args.player.id
    end
end


function Common:IsInPlay(cardID)
    local players = GetPlayers()
    for _, player in ipairs(players) do
        local zones = {player.treasures, player.units, {player.bond}}
        for _, zone in ipairs(zones) do
            for _, card in ipairs(zone) do
                if card.id == cardID then
                    return true
                end
            end
        end
    end
    return false
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

PipelineLayer = {}
function PipelineLayer.New(delegate, id)
    local result = {delegate=delegate, id=id}
    return result
end

Pipeline = {}
function Pipeline.New()
    local result = {
        layers = {},
        collectFunc = nil,
        collectInit = 0,
        result = 0
    }

    function result:AddLayer( delegate, id )
        id = id or nil
        self.layers[#self.layers+1] = PipelineLayer.New(delegate, id)
    end

    function result:Exec( ... )
        self.result = self.collectInit
        for i, layer in ipairs(self.layers) do
            local returned, success = layer.delegate(...)
            if not success then
                -- TODO
                return self.result, false
            end
            if self.collectFunc ~= nil then
                self.collectFunc(returned)
            end
        end
        return self.result, true
    end

    function result:Clear()
        self.layers = {}
    end

    function result:RemoveWithID( id )
        for i, layer in ipairs(self.layers) do
            if layer.id == id then
                table.remove(self.layers, i)
                return
            end
        end
        Log('WARN: TRIED TO REMOVE LAYER WITH ID '..' FROM TABLE, BUT FAILED')
    end

    return result
end


-- Card Creation
CardCreation = {}

function CardCreation:CardObject(props)
    local result = {}
    result.mutable = {} -- Things that power up the card
    result.labels = {}

    Common:RequireField(props, 'name')
    Common:RequireField(props, 'type')
    Common:RequireField(props, 'cost')
    Log('Creating card object for card '..props.name)

    result.name = props.name
    result.type = props.type
    result.cost = props.cost
    result.appendText = ''
    result.triggers = {}

    -- CanPlay pipeline
    result.CanPlayP = Pipeline.New()
    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:HasEnoughEnergy(result.cost)(player)
        end
    )
    function result:CanPlay(player)
        local _, res = self.CanPlayP:Exec(player)
        return res
    end

    -- PayCosts pipeline
    result.PayCostsP = Pipeline.New()
    result.PayCostsP:AddLayer(
        function (player)
            return nil, Common:PayEnergy(result.cost)(player)
        end
    )
    function result:PayCosts(player)
        local _, res = self.PayCostsP:Exec(player)
        return res
    end


    -- Play pipeline
    result.PlayP = Pipeline.New()
    result.PlayP:AddLayer(
        function (player)
            Log('Player '..player.name .. ' played card ' .. result.name)
            RegisterLastPlayer(result.id, player.name)
            return nil, true
        end
    )
    function result:Play(player)
        self.PlayP:Exec(player)
    end

    -- TODO? needs pipeline
    result.PowerUpP = Pipeline.New()
    result.PowerUpP:AddLayer(
        function ()
            Log('Powering up '..self.id)
            for key, value in pairs(self.mutable) do
                local new = value.current + 1
                if new <= value.max then
                    self.mutable[key].current = new
                else
                    Log('Did not power up value '..key..': '..value.current..' is the max')
                end
            end
            return nil, true
        end

    )
    function result:PowerUp()
        result.PowerUpP:Exec()
    end

    -- TODO? needs pipeline
    result.CanPowerUpP = Pipeline.New()
    result.CanPowerUpP:AddLayer(
        function ()
            return nil, Utility:TableLength(self.mutable) > 0
        end
    )
    function result:CanPowerUp()
        local _, r = result.CanPowerUpP:Exec()
        return r
    end

    -- TODO? needs pipeline
    result.PowerDownP = Pipeline.New()
    result.PowerDownP:AddLayer(
        function ()
            Log('Powering up '..self.id)
            for key, value in pairs(self.mutable) do
                local new = value.current - 1
                if new >= value.min then
                    self.mutable[key].current = new
                else
                    Log('Did not power down value '..key..': '..value.current..' is the min')
                end
            end
            return nil, true
        end
    )
    function result:PowerDown()
        result.PowerDownP:Exec()
    end

    -- TODO? needs pipeline
    result.CanPowerDownP = Pipeline.New()
    result.CanPowerDownP:AddLayer(
        function ()
            return nil, Utility:TableLength(self.mutable) > 0
        end
    )
    function result:CanPowerDown()
        local _, r = result.CanPowerDownP:Exec()
        return r
    end

    function result:AddKeyword(keywordName)
        local keywordData = Keywords.Map[keywordName]
        keywordData.modFunc(self)
    end

    function result:HasLabel(label)
        print(label)
        print(self)
        print(Utility:TableToStr(self.labels))
        for _, l in ipairs(self.labels) do
            if label == l then
                return true
            end
        end
        return false
    end

    return result
end


function CardCreation:Spell(props)
    local result = CardCreation:CardObject(props)

    -- Spell effect pipeline
    result.EffectP = Pipeline.New()
    result.EffectP:AddLayer(
        function(player)
            Log('Spell effect of ' .. result.name .. ', played by ' .. player.name)
            return nil, {}
        end
    )
    function result:Effect(player)
        self.EffectP:Exec(player)
    end

    result.PlayP:AddLayer(
        function (player)
            Emit(TRIGGERS.SPELL_CAST, {
                card = result,
                caster = player
            })
            result:Effect(player)
            PlaceIntoDiscard(result.id, player.id)
            return nil, true
        end
    )

    return result
end


function CardCreation:Source(props)
    props.cost = 0
    local result = CardCreation:Spell(props)

    result.EffectP:Clear()
    result.EffectP:AddLayer(
        function (player)
            IncreaseMaxEnergy(player.id, 1)
            return nil, true
        end
    )

    result.CanPlayP:Clear()
    result.CanPlayP:AddLayer(
        function (player)
            return nil, player.shared.sourceCount > 0
        end
    )

    result.PayCostsP:Clear()
    result.PayCostsP:AddLayer(
        function (player)
            player.shared.sourceCount = player.shared.sourceCount - 1
            return nil, true
        end
    )

    return result

end


function CardCreation:InPlay(props)
    local result = CardCreation:CardObject(props)

    -- LeavePlay pipeline
    result.LeavePlayP = Pipeline.New()
    result.LeavePlayP:AddLayer(
        function (player)
            Log('Card ' .. result.name .. ', controlled by ' .. player.name .. ', is leaving play')
            return nil, true
        end
    )
    function result:LeavePlay(player)
        result.LeavePlayP:Exec(player)
    end

    -- OnEnter pipeline
    result.OnEnterP = Pipeline.New()
    result.OnEnterP:AddLayer(
        function (player)
            Log('Called OnEnter func of '..result.name)
            return nil, true
        end
    )
    function result:OnEnter(player)
        result.OnEnterP:Exec(player)
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

    result.LeavePlayP:AddLayer(
        function (player)
            result.life = result.baseLife
            return nil, true
        end
    )

    return result
end


function CardCreation:Treasure(props)
    local result = CardCreation:Damageable(props)

    result.PlayP:AddLayer(
        function (player)
            PlaceInTreasures(result.id, player.id)
            return nil, true
        end
    )

    return result
end


function CardCreation:Unit(props)
    Common:RequireField(props, 'power')

    local result = CardCreation:Damageable(props)

    result.power = props.power
    result.basePower = props.power

    result.PlayP:AddLayer(
        function (player)
            RequestPlaceInUnits(result.id, player.id)
            return nil, true
        end
    )

    result.LeavePlayP:AddLayer(
        function (player)
            self.power = self.basePower
            return nil, true
        end
    )

    -- PreDeath pipeline
    result.PreDeathP = Pipeline.New()
    result.PreDeathP:AddLayer(
        function ()
            Log('Called PreDeath method of '..result.name)
            return nil, true
        end

    )
    function result:PreDeath()
        result.PreDeathP:Exec()
    end

    return result
end


PowerP = Pipeline.New()
PowerP:AddLayer(
    function (card)
        return card.power, true
    end
)

PowerP.collectFunc = function (returned)
    PowerP.result = PowerP.result + returned
end

function PowerOf(card)
    local result, _ = PowerP:Exec(card)
    return result
end

-- local card = CardCreation:Unit({name='Unit1', power=2, life=2})
-- card:AddConditional()
--     :WhileOwnerLifeGreaterThan(5)
--     :IncreasePower(2)


Keywords = {}
Keywords.Map = {
    virtuous = {
        modFunc = function (card)
            card.labels[#card.labels+1] = 'virtuous'

            card.PlayP:AddLayer(
                function (player)
                    local c = SummonCard(player.id, 'starters', 'Healing Light')
                    PlaceIntoHand(c.id, player.id)
                    return nil, true
                end
            )
        end
    },
    evil = {
        modFunc = function (card)
            card.labels[#card.labels+1] = 'evil'

            card.PlayP:AddLayer(
                function (player)
                    local c = SummonCard(player.id, 'starters', 'Corrupting Darkness')
                    PlaceIntoHand(c.id, player.id)
                    return nil, true
                end
            )
        end
    },
    fast = {
        modFunc = function (card)
            card.labels[#card.labels+1] = 'fast'

            card.OnEnterP:AddLayer(
                function (player)
                    for i, lane in ipairs(player.lanes) do
                        if lane.isSet and lane.unit.id == card.id then
                            -- TODO change to max available attacks
                            SetAvailableAttacks(player.id, card.id, 1)
                            break
                        end
                    end
        
                    return nil, true
                end
            )
        end
    },
    -- ingores defending unit
    idu = {
        modFunc = function (card)
            card.labels[#card.labels+1] = 'idu'
        end
    }
}