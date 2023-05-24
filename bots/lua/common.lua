Util = {}


function Util:TableToStr(t)
    if type(t) == 'table' then
        local s = '{ '
        for k,v in pairs(t) do
            if type(k) ~= 'number' then k = '"'..k..'"' end
            s = s .. '['..k..'] = ' .. Util:TableToStr(v) .. ','
        end
        return s .. '} '
    else
        return tostring(t)
    end
end

return Util