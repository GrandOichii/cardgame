

function _CreateCard( props )
    props.cost = 3

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
    function result:Effect( player )
        prevEffect(self, player)

        local target = Common.Targeting:Treasure('Select target Treasure for '..self.name, player.id)
        DealDamage(self.id, target.id, 1)
    end

    return result
end