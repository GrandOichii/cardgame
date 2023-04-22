

function _CreateCard( props )
    props.cost = 3

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect( player )
        prevEffect(self, player)

        local target = Common:TargetTreasure(player.id)
        DealDamage(self.id, target.id, 1)
    end

    return result
end