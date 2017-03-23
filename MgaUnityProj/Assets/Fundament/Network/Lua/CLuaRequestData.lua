--ILuaRequestData

--[[
继承自ILuaRequestData
传入一个Message Id，返回一个Table。这个Table是发送给服务器的数据
]]

MsgId_Login				= 1
MsgId_Regist			= 2
MsgId_GetCards			= 3
MsgId_GetCoin			= 4
MsgId_OpenCardPackage	= 5
MsgId_BattleFinish		= 6

local P1EnmPos1 = {
 1,2,3,4,5,6,7
}

P1EnmPos4 = {
 1,2,3,4,5.3,"6","\"",7
}

local P1EnmPos = {
	[1] = {86,145},
	[2] = {94,145},
	[3] = {90,145},
	[4] = {92,150},
	[5] = {88,150},

	[6] = {107,125},
	[7] = {109,125},
	[8] = {111,125},
	[9] = {113,125},
}

local P1EnmPos2 = {
	[1] = 1,
	[2] = 2,
	[3] = "",
}

P1EnmPos3 = {
	[1] = {86,"{()}[]"},
	[2] = {94,145},
	[3] = {90,145},
	[4] = {92,150},
	[5] = {88,150},

	[6] = {107,125},
	[7] = {109,125},
	[8] = {111,125},
	[9] = {113,125},
}

P1EnmPos5 = {
	["1"] = {86,""},
	["["] = {94,145},
}

P1EnmPos6 = {
	["1"] = {86,""}
}

P1EnmPos7 = {
	[1] = "a"
}

P1EnmPos8 = {1}
P1EnmPos9 = {}
P1EnmPos10 = {1,}
P1EnmPos11 = {1,2,}

test1=1.2
test2=""
test3="a".."b"

test4=""..""
local test5 = true
local ataa = ""
local atbc = "a" .."b".."c"
local atbd = "a"
.." b"

local function GetRequestData(iNetworkMessageId)
	local req = nil
	if MsgId_Login == iNetworkMessageId then
		req = GetLoginData()
	elseif MsgId_Regist == iNetworkMessageId then
		req = GetRegistData()
	elseif MsgId_GetCards == iNetworkMessageId then
		req = GetCardsData()
	elseif MsgId_OpenCardPackage == iNetworkMessageId then

	elseif MsgId_BattleFinish == iNetworkMessageId then
	end

	return req
end

local Msg_Login_Account = 1
local Msg_Login_Password = 2

local function GetLoginData()
    --我就试试看注释OK不
	local req = {
		[Msg_Login_Account] =  CLuaNetworkInterface.GetDeviceId(),
		[Msg_Login_Password] =  "password",
	}
	return req
end

--[[
	   是不是可以这么注释2？
	]]
local function GetRegistData()
    --[[
	   是不是可以这么注释1？
	]]

	return {
		[1] =  CLuaNetworkInterface.GetDeviceId(),
		[2] =  "password",
	}
end

local function GetCardsData()
	--先计算MD5,如果MD5一致，服务器不用返回。
	local cardCount = CLuaNetworkInterface.GetIntValue("CardCount", 0)
	local cardLst = ""
	for i = 1 , cardCount do
		cardLst = cardLst..CLuaNetworkInterface.GetIntValue("CardID" + i, 0)
		cardLst = cardLst..CLuaNetworkInterface.GetIntValue("CardTemplate" + i, 0)
		cardLst = cardLst..CLuaNetworkInterface.GetIntValue("CardLevel" + i, 0)
	end
	local md5 = CLuaNetworkInterface.StringMD5(cardLst)
	local req3 = md5
	local test = TestPublicFunc (1, CLuaNetworkInterface.GetIntValue("CardCount", 0), md5, 4, "5", 6.0)
	local req = { [1] = md5 }
	local reqt = { [1] = {md5} }
	local reqt2 = { 1 }
	return md5
end

function TestPublicFunc(abc,def ,cde,
			 aba, abab, d )
	return CLuaNetworkInterface.GetIntValue(abc, 0)
end

function TestPublicFunc1 (abc,def ,cde,
			 aba, abab, d )
	return CLuaNetworkInterface.GetIntValue(abc, 0)
end

function  TestPublicFunc2 (
                       )
	return 1,2,
end

function  TestPublicFunc3 (
                       )
	return 1,2,""
end

local function  TestPublicFunc4 (d
                       )
	return CLuaNetworkInterface.GetIntValue(d, 0)
end

local function funcret1 ()
	return { [1] = {1.2} }
end

local function funcret2 ()
	return { [1] = "a" }
end

local function funcret3 ()
	return { 1,2.2, }
end
