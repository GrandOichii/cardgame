
-- TODO not tested
-- TODO change to pipeline
function _CreateCard(props)
    props.cost = 1
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)

    result.CanPowerUpP:Clear()
    result.CanPowerUpP:AddLayer(
        function ()
            return nil, true
        end
    )

    result.PowerUpP:AddLayer(
        function ()
            result.power = result.power + 2
            result.life = result.life + 2
        end
    )

    return result
end