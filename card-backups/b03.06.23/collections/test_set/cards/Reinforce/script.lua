

-- TODO untested
function _CreateCard(props)
    props.cost = 5

    local result = CardCreation:Spell(props)

    function result:Effect(player)
        for _, unit in ipairs(player.units) do
            unit.power = unit.power + 3
            unit.life = unit.life + 3
        end
    end

    return result
end