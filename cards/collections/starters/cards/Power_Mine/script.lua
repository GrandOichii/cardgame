
-- TODO not tested
function _CreateCard(props)
    props.cost = 2
    props.life = 2

    local result = CardCreation:Treasure(props)

    result.mutable.charge = {
        current = 1,
        min = 0,
        max = 0
    }

    local prevPowerDown = result.PowerDown
    function result:PowerDown()
        prevPowerDown(self)

        if self.mutable.charge.current == 0 then
            local owner = GetController(self.id)
            Destroy(self.id)
            local target = Common.Targeting:Unit(owner.id)
            Destroy(target.id)
        end
    end
    
    return result
end