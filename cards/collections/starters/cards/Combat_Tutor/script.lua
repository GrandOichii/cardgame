

function _CreateCard(props)
    props.cost = 3
    props.life = 2
    props.power = 2

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            local units = player.units
            for _, unit in ipairs(units) do
                if unit ~= nil and unit.id ~= result.id then
                    if unit:CanPowerUp() then
                        unit:PowerUp()
                    else
                        unit.life = unit.life + 1
                        unit.power = unit.power + 1
                    end
                end
            end
            return nil, true
        end
    )

    return result
end