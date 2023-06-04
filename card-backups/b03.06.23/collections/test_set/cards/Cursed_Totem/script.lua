
-- TODO not tested
function _CreateCard(props)
    props.cost = 5
    props.life = 7

    local result = CardCreation:Treasure(props)
    result.mutable.damageDealt = {
        min = 1,
        current = 1,
        max = 3
    }
    
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Effect(function (player, args)
            local wasCount = #player.units
            local opponent = OpponentOf(player.id)
            local units = {table.unpack(player.units), table.unpack(opponent.units)}
            for _, unit in ipairs(units) do
                DealDamage(result.id, unit.id, result.mutable.damageDealt.current)
            end
            local controller = GetController(result.id)
            if wasCount == #controller.units then
                return
            end
            Destroy(result.id)
            local c = SummonCard(player.id, 'test_set', 'Angry Spirit')
            RequestPlaceInUnits(c.id, player.id)
        end)
        :Build()
    
    return result
end