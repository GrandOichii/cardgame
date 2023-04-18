
-- TODO not tested
function _CreateCard(props)
    local result = CardCreation:Bond(props)

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_END)
        :Zone(ZONES.UNITS)
        :Effect(function (player, args)
            local discard = player.discard
            local cards = {}
            for _, card in ipairs(discard) do
                if card.type == 'Unit' and card.cost <= 3 then
                    cards[#cards+1] = card
                end
            end
            if #cards == 0 then
                return
            end
            local card = cards[math.random(#cards)]
            RemoveFromDiscard(card.id, player.id)
            PlaceIntoHand(card.id, player.id)
        end)
        :Build()

    return result
end