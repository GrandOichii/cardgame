
-- TODO not tested
function _CreateCard(props)
    props.cost = 1
    local result = CardCreation:Spell(props)
    result.mutable.spellDamage = {
        min = 1,
        current = 1,
        max = 2
    }

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local target = Common.Targeting:UnitOrTreasure('Select target for '..self.name, player.id)
        DealDamage(self.id, target.id, self.mutable.spellDamage.current)
        if Common:IsInPlay(target.id) then
            return
        end
        DrawCards(player.id, 1)
    end

    return result
end