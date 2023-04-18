
-- TODO not tested
function _CreateCard(props)
    props.cost = 7

    local result = CardCreation:Spell(props)

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local players = GetPlayers()
        for _, p in ipairs(players) do
            for _, treasure in ipairs(p.treasures) do
                Destroy(treasure.id)
            end
        end
    end

    return result
end