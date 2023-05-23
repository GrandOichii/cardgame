

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
            result:PowerUp()
            local damage = result.mutable.damageMyself.current
            DealDamage(result.id, result.id, damage)
        end)
        :Build()

    -- TODO replace with on destroy
    result.LeavePlayP:AddLayer(
        function (player)
            local players = GetPlayers()
            for _, p in ipairs(players) do
                DealDamageToPlayer(result.id, p.id, result.mutable.damageAll.current)
                for _, unit in ipairs(p.units) do
                    DealDamage(result.id, unit.id, result.mutable.damageAll.current)
                end
            end
        end
    )

    return result
end