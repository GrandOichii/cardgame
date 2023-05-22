
-- TODO not tested
function _CreateCard(props)
    props.cost = 6
    props.power = 6
    props.life = 5

    local result = CardCreation:Unit(props)
    result.mutable.powerUpAmount = {
        min = 2,
        current = 3,
        max = 5
    }

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            local players = GetPlayers()
            local all = {}
            for _, p in ipairs(players) do
                local cards = Common:FilterInPlay(p, function (card) return card:CanPowerUp() end)
                all = {table.unpack(all), table.unpack(cards)}
            end
            for i = 1, result.mutable.powerUpAmount.current do
                if #all == 0 then
                    return
                end
                local cI = math.random( #all )
                local card = table.remove(all, cI)
                card:PowerUp()
            end
        end)
        :Build()
    
    return result
end