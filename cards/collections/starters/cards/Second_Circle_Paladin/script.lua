
-- TODO not tested
function _CreateCard(props)
    local requiredCardName = 'Healing Light'

    props.cost = 3
    props.power = 6
    props.life = 6

    local result = CardCreation:Unit(props)

    result.OnEnterP:AddLayer(
        function (player)
            if not Common:HasCardsInHand(requiredCardName, player) then
                result.life = result.life - 3
                result.power = result.power - 3
                return
            end
            local target = Common.Targeting:CardInHand('Choose a card to discard to '..result.name, player, function(card) return card.name == requiredCardName end)
            if target == nil then
                result.life = result.life - 3
                result.power = result.power - 3
                return
            end
            RemoveFromHand(target.id, player.id)
            PlaceIntoDiscard(target.id, player.id)
            return nil, true
        end
    )

    return result
end