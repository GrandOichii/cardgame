
-- TODO not tested
function _CreateCard(props)
    local requiredCardName = 'Healing Light'

    props.cost = 3
    props.power = 6
    props.life = 6

    local result = CardCreation:Unit(props)

    local prevOnEnter = result.OnEnter
    function result:OnEnter(player)
        prevOnEnter(self, player)
        if not Common:HasCardsInHand(requiredCardName) then
            self.life = self.life - 3
            self.power = self.power - 3
            return
        end
        local target = Common.Targeting:CardInHand('Choose a card to discard to '..self.name, player, function(card) return card.name == requiredCardName end)
        if target == nil then
            self.life = self.life - 3
            self.power = self.power - 3
            return
        end
        RemoveFromHand(target.id, player.id)
        PlaceIntoDiscard(target.id, player.id)
    end

    return result
end