
-- TODO not tested
function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)
    result.mutable.howMany = {
        min = 1,
        current = 2,
        max = 4
    }

    result.CanPlayP:AddLayer(
        function (player)
            -- TODO? too clunky
            return nil, Common:PowerUpCardInPlay()
        end
    )

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
            for i = 1, result.mutable.howMany.current do
                target:PowerUp()
            end
            return nil, true
        end
    )

    return result
end