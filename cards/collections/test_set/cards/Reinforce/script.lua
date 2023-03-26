

-- TODO untested
function _CreateCard(props)
    props.cost = 4

    local result = CardCreation:Spell(props)

    function result:Effect(player)
        for _, unit in ipairs(player.units) do
            unit.power = unit.power + 4
            unit.life = unit.life + 4
        end
    end

    return result
end