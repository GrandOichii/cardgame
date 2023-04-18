package.path = package.path..';../bots/lua/?.lua'
local json = require 'json'
local common = require 'common'

local function stateFromJ(stateJ)
    local result = json.parse(stateJ)
    if result ~= nil then
        return result
    end
    return {}
end

-- prompt targets
function _Prompt(stateJ)
    local state = stateFromJ(stateJ)
    -- TODO
    return state.args[1]
end

local lastState = ''
local actionsThisTurn = {}

local function triedThisTurn(action)
    for _, a in ipairs(actionsThisTurn) do
        if action == a then
            return true
        end
    end
    return false
end

-- prompt actions (play, attack, etc.)
function _PromptAction(stateJ)
    local state = stateFromJ(stateJ)
    local me = state.players[state.myData.playerI+1]
    for _, card in ipairs(state.myData.hand) do
        if card.type == 'Source' then
            local action = 'play '..card.id
            if not triedThisTurn(action) then
                actionsThisTurn[#actionsThisTurn+1] = action
                return action
            end
        end
    end
    local toBePlayed = nil
    for _, card in ipairs(state.myData.hand) do
        if me.energy >= card.cost and not triedThisTurn('play '..card.id) and (toBePlayed == nil or (card.cost > toBePlayed.cost)) then
            toBePlayed = card
        end
    end
    if toBePlayed ~= nil then
        local result = 'play '..toBePlayed.id
        actionsThisTurn[#actionsThisTurn+1] = result
        return result
    end

    -- -- play a Source card if can
    -- for _, card in ipairs(state.myData.hand) do
    --     if card.type == 'Source' then
    --         local result = 'play '..card.id
    --         if not (stateJ == lastState and lastAction == result) then
    --             lastAction = result
    --             lastState = stateJ
    --             return result
    --         end
    --     end
    -- end

    -- -- play the most expensive card can cast
    -- local toBePlayed = nil
    -- for _, card in ipairs(state.myData.hand) do
    --     if toBePlayed == nil or
    --         (card.cost > toBePlayed.cost and
    --         me.energy >= card.cost and
    --         not (stateJ == lastState and lastAction == 'play '..card.id))
    --     then
    --         toBePlayed = card
    --     end
    -- end
    -- if toBePlayed ~= nil then
    --     local result = 'play '..toBePlayed.id
    --     lastAction = result
    --     lastState = stateJ
    --     return result
    -- end

    return 'pass'
end

-- prompt to choose lane
function _PromptLane(stateJ)
    local state = stateFromJ(stateJ)
    local me = state.players[state.myData.playerI+1]
    if state.request == 'pick' then
        -- TODO
        return 1
    end
    print(common:TableToStr(me.units))
    for i, unit in ipairs(me.units) do
        if unit.card == nil then
            return i - 1
        end
    end
    return 1
end

-- simple update function
function _Update(stateJ)
    local state = stateFromJ(stateJ)
    if state.players[state.myData.playerI+1] == state.curPlayerI then
        return
    end

    actionsThisTurn = {}
end
