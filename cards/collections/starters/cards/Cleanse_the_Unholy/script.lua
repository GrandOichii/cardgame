

-- TODO not tested
function _CreateCard(props)
    props.cost = 5

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local players = GetPlayers()
            for _, p in ipairs(players) do
                local cards = {table.unpack(p.treasures), table.unpack(p.units)}
                for _, card in ipairs(cards) do
                    if card:HasLabel('evil') then
                        Destroy(card.id)
                    end
                end
            end
            return nil, true
        end
    )

    return result
end