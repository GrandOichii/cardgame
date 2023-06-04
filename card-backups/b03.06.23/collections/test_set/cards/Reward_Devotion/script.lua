

function _CreateCard(props)
    local requiredCardName = 'Healing Light'

    props.cost = 2

    local result = CardCreation:Spell(props)

    result.EffectP:AddLayer(
        function (player)
            local target = Common.Targeting:CardInHand('Choose a card to discard to '..result.name, player, function(card) return card.name == requiredCardName end)
            if target ~= nil then
                RemoveFromHand(target.id, player.id)
                PlaceIntoDiscard(target.id, player.id)
                IncreaseMaxEnergy(player.id, 1)
            end
            return nil, true
        end
    )

    result.CanPlayP:AddLayer(
        function (player)
            return nil, Common:HasCardsInHand(requiredCardName, player)
        end
    )

    return result
end