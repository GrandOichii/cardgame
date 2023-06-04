
function _CreateCard(props)
    props.cost = 3

    local result = CardCreation:Spell(props)
    result.mutable.cardDraw = {
        min = 3,
        current = 3,
        max = 4
    }
    result.mutable.cardDiscard = {
        min = 1,
        current = 1,
        max = 2
    }

    result.EffectP:AddLayer(
        function (player)
            DrawCards(player.id, result.mutable.cardDraw.current)
            player = PlayerByID(player.id)
            Common:DiscardCards('Discard a card to '..result.name, player, result.mutable.cardDiscard.current)
            return nil, true
        end
    )

    return result
end