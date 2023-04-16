
-- TODO untested
function _CreateCard(props)
    props.cost = 4

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local opponent = OpponentOf(player.id)
        DealDamageToPlayer(self.id, opponent.id, 5)
    end

    return result
end