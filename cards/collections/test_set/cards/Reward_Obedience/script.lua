

function _CreateCard(props)
    local requiredCardName = 'Corrupting Darkness'
    props.cost = 2

    local result = CardCreation:Spell(props)

    local prevCanPlay = result.CanPlay
    function result:CanPlay(player)
        if not prevCanPlay(self, player) then
            return false
        end
        return Common:HasCardsInHand(requiredCardName, player)
    end

    local prevEffect = result.Effect
    function result:Effect(player)
        prevEffect(self, player)

        local target = Common.Targeting:CardInHand('Choose a card to discard to '..self.name, player, function(card) return card.name == requiredCardName end)
        if target ~= nil then
            RemoveFromHand(target.id, player.id)
            PlaceIntoDiscard(target.id, player.id)
            IncreaseMaxEnergy(player.id, 1)
        end
    end

    return result
end