
function _CreateCard(props)
    props.cost = 1

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:Target('Select target for '..result.name, player.id, {
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
            }, result.id)
            target:PowerUp()
            return nil, true
        end
    )

    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:PowerUpCardInPlay()
        end
    )

    return result
end