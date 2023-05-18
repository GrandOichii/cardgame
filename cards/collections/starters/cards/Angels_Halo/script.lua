
-- TODO not tested
function _CreateCard(props)
    props.cost = 7
    props.life = 5

    local result = CardCreation:Treasure(props)
    result:AddKeyword('virtuous')

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Cost(Common:NoCost())
        :Effect(function (player, args)
            local hand = {}
            for _, card in ipairs(player.hand) do
                if card.name ~= 'Healing Light' then
                    hand[#hand+1] = card
                end
            end
            local card = hand[math.random(#hand)]
            card:AddKeyword('virtuous')
            card.appendText = card.appendText..'\n{Virtuous}'
        end)
    :Build()
    
    return result
end