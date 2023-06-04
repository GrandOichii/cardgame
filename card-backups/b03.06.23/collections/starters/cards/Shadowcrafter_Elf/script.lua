function _CreateCard(props)
    local requiredCardName = 'Corrupting Darkness'
    props.cost = 2
    props.power = 4
    props.life = 4

    local result = CardCreation:Unit(props)
    
    result.OnEnterP:AddLayer(
        function (player)
            if not Common:HasCardsInHand(requiredCardName, player) then
                Destroy(result.id)
                return
            end
    
            local target = Common.Targeting:CardInHand('Choose a card to discard to '..result.name, player, function(card) return card.name == requiredCardName end)
            if target == nil then
                Destroy(result.id)
                return
            end
            RemoveFromHand(target.id, player.id)
            PlaceIntoDiscard(target.id, player.id)
            IncreaseMaxEnergy(player.id, 1)
            return nil, true
        end
    )

    return result
end