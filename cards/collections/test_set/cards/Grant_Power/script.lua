

function _CreateCard(props)
    props.cost = 1

    local result = CardCreation:Spell(props)

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
        target:PowerUp()
    end

    return result
end