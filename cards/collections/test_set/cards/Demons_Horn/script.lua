
-- TODO not tested
function _CreateCard(props)
    props.cost = 5
    local result = CardCreation:Treasure(props)
    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common.AlwaysTrue)
        :IsSilent(false)
        :On(TRIGGERS.CARD_DRAW)
        :Zone(ZONES.TREASURES)
        :Cost(Common:NoCost())
        :Effect(function (player, args)
            LoseLife(args.player.id, args.amount)
        end)
        :Build()

    return result
end