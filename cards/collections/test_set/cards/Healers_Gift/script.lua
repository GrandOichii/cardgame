

function _CreateCard(props)
    local result = CardCreation:Bond(props)
    -- result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
    --     :Check(Common:IsOwnersTurn(result))
    --     :IsSilent(false)
    --     :On(TRIGGERS.TURN_START)
    --     :Zone(ZONES.BOND)
    --     :Cost(Common:NoCost())
    --     :Effect(function (player, args)
    --         GainLife(player.id, 1)
    --     end)
    --     :Build()
    return result
end