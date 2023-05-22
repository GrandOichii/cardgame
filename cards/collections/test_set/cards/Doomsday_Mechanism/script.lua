

-- TODO not tested
function _CreateCard(props)
    props.cost = 6
    props.life = 8

    local result = CardCreation:Treasure(props)
    result.mutable.damageMyself = {
        min = 1,
        current = 1,
        max = 5
    }
    result.mutable.damageAll = {
        min = 0,
        current = 2,
        max = 7
    }

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Effect(function (player, args)
            local damage = result.mutable.damageMyself.current
            DealDamage(result.id, result.id, damage)
        end)
        :Build()

    -- TODO replace with on destroy
    local prevLeave = result.LeavePlay
    function result:LeavePlay(player)
        prevLeave(self, player.id)
        local players = GetPlayers()
        for _, p in ipairs(players) do
            DealDamageToPlayer(self.id, p.id, self.mutable.damageAll.current)
            for _, unit in ipairs(p.units) do
                DealDamage(self.id, unit.id, self.mutable.damageAll.current)
            end
        end

    end

    return result
end