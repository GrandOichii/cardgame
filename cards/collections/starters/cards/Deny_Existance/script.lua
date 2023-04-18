
-- TODO not tested
function _CreateCard(props)
    props.cost = 5

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local target = Common:TargetUnit(player.id)
        Destroy(target.id)
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