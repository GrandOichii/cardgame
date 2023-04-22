

function _CreateCard(props)
    props.cost = 2

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local unit = Common.Targeting:Unit(player.id)
        DealDamage(self.id, unit.id, 3)
    end

    local prevCanPlay = result.CanPlay
    function result:CanPlay(player)
        local prev = prevCanPlay(self, player)
        if not prev then
            return false
        end
        return Common:AtLeastOneUnitInPlay()
    end

    return result
end