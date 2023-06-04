

function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)
    result.mutable.cardDraw = {
        current = 2,
        min = 1,
        max = 4
    }

    result.EffectP:AddLayer(
        function (player)
            DrawCards(player.id, result.mutable.cardDraw.current)
            return nil, true
        end
    )

    return result
end