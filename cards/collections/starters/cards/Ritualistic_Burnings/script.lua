

function _CreateCard(props)
    props.cost = 2
    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local target = Common:TargetTreasure(player.id)
        print(target)
        DealDamage(self.id, target.id, 3)
        DealDamageToPlayer(self.id, GetController(target.id).id, 3)
    end

    return result
end