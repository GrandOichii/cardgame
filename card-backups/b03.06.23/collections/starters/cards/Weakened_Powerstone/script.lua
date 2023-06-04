
-- TODO change to "Units you control gain +[mutable] power."
-- TODO not tested
function _CreateCard(props)
    props.cost = 3
    props.life = 5

    local result = CardCreation:Treasure(props)
    result.mutable.power = {
        min = 0,
        current = 2,
        max = 3
    }

    local powerLayerID = nil
    result.OnEnterP:AddLayer(
        function (player)
            powerLayerID = result.id
            PowerP:AddLayer(function( card )
                local add = 0
                local myCID = player.id
                local otherCID = GetController(card.id).id
                if otherCID == myCID then
                    add = 2
                end
                return add, true
            end, powerLayerID)
            return nil, true
        end
    )
    result.LeavePlayP:AddLayer(
        function (player)
            PowerP:RemoveWithID(powerLayerID)
            return nil, true
        end
    )

    result.triggers[#result.triggers+1] = EffectCreation:TriggerBuilder()
        :Check(Common:IsOwnersTurn(result))
        :Cost(Common:NoCost())
        :IsSilent(false)
        :On(TRIGGERS.TURN_START)
        :Zone(ZONES.TREASURES)
        :Effect(function (player, args)
            result:PowerDown()
            if result.mutable.power.current == 0 then
                Destroy(result.id)
            end
        end)
        :Build()

    return result
end