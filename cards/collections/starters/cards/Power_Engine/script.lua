

-- TODO not tested
function _CreateCard(props)
    local result = CardCreation:Bond(props)

    result.mutable.power = {
        min = 0,
        current = 3,
        max = 10
    }

    local powerLayerID = nil
    result.OnEnterP:AddLayer(
        function (player)
            powerLayerID = result.id
            PowerP:AddLayer(function( card )
                local add = 0
                local myCID = player.id
                local otherCID = GetController(card.id).id
                if result.mutable.power.current == 10 and otherCID == myCID then
                    add = 1
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

    return result
end