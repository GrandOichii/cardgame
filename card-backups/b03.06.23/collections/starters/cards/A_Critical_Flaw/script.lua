
-- TODO not tested
function _CreateCard(props)
    props.cost = 7

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local players = GetPlayers()
            for _, p in ipairs(players) do
                for _, treasure in ipairs(p.treasures) do
                    Destroy(treasure.id)
                end
            end
            return nil, true
        end
    )

    return result
end