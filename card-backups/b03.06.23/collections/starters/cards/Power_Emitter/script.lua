

-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.life = 3

    local result = CardCreation:Treasure(props)
    result.mutable.powerUpAmount = {
        min = 0,
        current = 1,
        max = 2
    }

    local check = function (player, args)
        if not Common:IsOwnersTurn(result)(player, args) then
            return false
        end
        local targets = {}
        for _, p in ipairs(GetPlayers()) do
            local cards = {table.unpack(p.units), table.unpack(p.treasures)}
            for _, card in ipairs(cards) do
                if card:CanPowerUp() then
                    targets[#targets+1] = card
                end
            end
        end
        return #targets > 0
    end

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(check)
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Effect(function (player, args)
            local filter = Common.Targeting.Selectors:Filter(function (card) return card:CanPowerUp() end)
            local target = Common.Targeting:Target('Select target Unit or Treasure for '..result.name, player.id, {
                {
                    what = 'treasures',
                    which = filter
                },
                {
                    what = 'units',
                    which = filter
                }
            }, result.id)
            for _ = 1, result.mutable.powerUpAmount.current do
                target:PowerUp()
            end
        end)
        :Build()

    return result
end