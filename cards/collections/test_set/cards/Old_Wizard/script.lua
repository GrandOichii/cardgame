

function _CreateCard(props)
    props.attack = 1
    props.health = 2
    props.cost = 2
    local card = CardCreation:Creature(props)
    -- card.triggers
        -- .Zone('in_play')
    card.triggers[#card.triggers+1] = EffectCreation.TriggerBuilder:Create()
        :Zone(ZONES.HAND)
        :IsSilent(true)
        :On(TRIGGERS.TURN_START)
        :EffectF(function (...)
            print()
            print('TURN START TRIGGER')
            print('\t' .. Utility:TableToStr(...))
            print()
        end)
        :Build()
        -- card.triggers
    --     :On(TRIGGERS.TURN_START)
    --     :Zone(ZONES.HAND)
    --     :Add(true, nil, function (args)
    --         print()
    --         print('TURN START TRIGGER')
    --         print('\t' .. Utility:TableToStr(args))
    --         print()
    --     end)


    -- card.triggers
    --     :On(TRIGGERS.TURN_END)
    --     :Zone(ZONES.HAND)
    --     :Add(false, nil, function (args)
    --         print()
    --         print('TURN END TRIGGER')
    --         print('\t' .. Utility:TableToStr(args))
    --         print()
    --     end)

    -- print(Utility:TableToStr(card.triggers))
    return card
end

