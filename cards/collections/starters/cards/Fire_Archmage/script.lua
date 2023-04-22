

function _CreateCard(props)
    props.cost = 5
    -- props.cost = 1
    props.power = 4
    props.life = 4

    local result = CardCreation:Unit(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersSpell(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.SPELL_CAST)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            local target = Common.Targeting:UnitOrTreasure('Select target Unit or Treasure for '..result.name, player.id)
            DealDamage(result.id, target.id, 4)
        end)
        :Build()

    return result
end