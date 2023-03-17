

function _CreateCard(props)
    local card = CardCreation:Source(props)

    print(Utility:TableToStr(card))
    return card
end

