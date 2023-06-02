
-- TODO not tested
function _CreateCard(props)
    props.cost = 2
    props.life = 2

    local result = CardCreation:Treasure(props)

    result.mutable.charge = {
        current = 0,
        min = 0,
        max = 1
    }

    result.PowerUpP:AddLayer(
        function ()
            if result.mutable.charge.current == 1 then
                local owner = GetController(result.id)
                Destroy(result.id)
                local target = Common.Targeting:Unit('Select target Unit or Treasure for '..result.name, owner.id, result.id)
                Destroy(target.id)
            end
        end
    )
    
    return result
end