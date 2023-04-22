

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
        -- TODO? force player to choose what card to discard?

        for _, card in ipairs(player.hand) do
            if card.name == requiredCardName then
                RemoveFromHand(card.id, player.id)
                PlaceIntoDiscard(card.id, player.id)
                IncreaseMaxEnergy(player.id, 1)
                break
            end
        end
    end

    return result
end