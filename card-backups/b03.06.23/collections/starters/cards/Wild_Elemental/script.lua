
-- TODO didn't work when casting spells
function _CreateCard(props)
    props.cost = 2
    props.power = 2
    props.life = 3

    local result = CardCreation:Unit(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersSpell(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.SPELL_CAST)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            result.life = result.life + 1
            result.power = result.power + 1
        end)
        :Build()

    return result
end