

function _CreateCard(props)
    props.cost = 3
    props.power = 3
    props.life = 2

    local result = CardCreation:Unit(props)

    -- local powerLayerID = nil
    -- result.OnEnterP:AddLayer(
    --     function (player)
    --         powerLayerID = result.id
    --         PowerP:AddLayer(function( card )
    --             local add = 0
    --             local myCID = GetController(card.id)
    --             if card.id ~= result.id and GetController(card.id).id == myCID then
    --                 add = 1
    --             end
    --             return add, true
    --         end)
    --         return nil, true
    --     end
    -- )
    -- result.LeavePlayP:AddLayer(
    --     function (player)
    --         PowerP:RemoveWithID(powerLayerID)
    --         return nil, true
    --     end
    -- )

    -- powermod
    local powerLayerID = nil
    result.OnEnterP:AddLayer(
        function (player)
            powerLayerID = result.id
            PowerP:AddLayer(function( card )
                local add = 0
                local myCID = player.id
                if card.id ~= result.id and GetController(card.id).id == myCID then
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