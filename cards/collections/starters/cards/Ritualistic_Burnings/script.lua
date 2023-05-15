

function _CreateCard(props)
    props.cost = 2
    local result = CardCreation:Spell(props)

    -- TODO not tested
    local prevCanPlay = result.CanPlay
    function result:CanPlay( player )
        local last = prevCanPlay(self, player)
        if not last then
            return false
        end
        return Common:AtLeastOneTreasureInPlay()
    end

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local target = Common.Targeting:Treasure('Select target Treasure for '..self.name, player.id, result.id)
        DealDamage(self.id, target.id, 3)
        DealDamageToPlayer(self.id, GetController(target.id).id, 3)
    end

    return result
end