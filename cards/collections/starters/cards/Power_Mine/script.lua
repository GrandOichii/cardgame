
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

    local prevPowerUp = result.PowerUp
    function result:PowerUp()
        prevPowerUp(self)

        if self.mutable.charge.current == 1 then
            local owner = GetController(self.id)
            Destroy(self.id)
            local target = Common.Targeting:Unit('Select target Unit or Treasure for '..self.name, owner.id, result.id)
            Destroy(target.id)
        end
    end
    
    return result
end