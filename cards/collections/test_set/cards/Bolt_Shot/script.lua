

function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)
    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local target = PickUnit(player.id)
        local unit = Common:TargetUnit(self.id)
        DealDamage(self.id, unit.id, 3)
    end
    return result
end