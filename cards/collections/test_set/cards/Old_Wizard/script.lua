

function _CreateCard(props)
    props.attack = 1
    props.health = 2
    local card = CardCreation:Creature(props)

    print(Utility:TableToStr(card))
    return card
end

