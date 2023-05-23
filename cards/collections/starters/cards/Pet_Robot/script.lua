
-- TODO not tested
-- TODO change to pipeline
function _CreateCard(props)
    props.cost = 1
    props.power = 1
    props.life = 1

    local result = CardCreation:Unit(props)

    function result:CanPowerUp()
        return true
    end

    local prevPowerUp = result.PowerUp
    function result:PowerUp()
        prevPowerUp(self)
        self.power = self.power + 2
        self.life = self.life + 2
    end

    return result
end