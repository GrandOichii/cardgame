

function _CreateCard(props)
    props.cost = 3
    props.life = 2
    props.power = 2

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        local units = player.units
        for _, unit in ipairs(units) do
            if unit ~= nil and unit. then
                
            end
        end
    end

    return result
end