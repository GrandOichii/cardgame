

-- TODO not tested
function _CreateCard(props)
    props.cost = 5

    local result = CardCreation:Spell(props)
    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)
        local players = GetPlayers()
        for _, p in ipairs(player) do
            local cards = {table.unpack(p.treasures), table.unpack(p.units)}
            for _, card in ipairs(cards) do
                if card:HasLabel('evil') then
                    Destroy(card.id)
                end
            end
        end
    end
end