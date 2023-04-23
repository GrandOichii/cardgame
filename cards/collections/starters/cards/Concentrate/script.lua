
-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    local result = CardCreation:Spell(props)
    result.mutable.howMany = {
        min = 1,
        current = 2,
        max = 4
    }

    local prevCanPlay = result.CanPlay
    function result:CanPlay(player)
        if not prevCanPlay(self, player) then
            return false
        end
        -- TODO too clunky
        return Common:PowerUpCardInPlay()
    end

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)

        local target = Common.Targeting:Target('Select target for '..self.name, player.id, {
            {
                what = 'bond',
                which = Common.Targeting.Selectors:Filter(function(card) return card:CanPowerUp() end)
            },
            {
                what = 'treasures',
                which = Common.Targeting.Selectors:Filter(function(card) return card:CanPowerUp() end)
            },
            {
                what = 'units',
                which = Common.Targeting.Selectors:Filter(function(card) return card:CanPowerUp() end)
            }
        })
        for i = 1, self.mutable.howMany.current do
            target:PowerUp()
        end
    end

    return result
end