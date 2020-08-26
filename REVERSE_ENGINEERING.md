# The reverse engineering of the MagicHome protocol
There are a few libraries out there which implement the client side of the [MagicHome](https://play.google.com/store/apps/details?id=com.zengge.wifi&hl=de) protocol, which is the app. But there wasn't (until now) a library which implements the "server" side, which would be the [controller](https://aliexpress.com/item/33035745736.html?src=google&albch=shopping&acnt=494-037-6276&isdl=y&slnk=&plac=&mtctp=&albbt=Gploogle_7_shopping&aff_atform=google&aff_short_key=UneMJZVf&&albagn=888888&albcp=1705854617&albag=67310370915&trgt=743612850714&crea=de33035745736&netw=u&device=c&albpg=743612850714&albpd=de33035745736&gclid=CjwKCAjwkJj6BRA-EiwA0ZVPVndgdbe_APMj71f5FiGF1x9L4pg2KP_3WflQhmYt4-kunBiLryhhrRoC_UwQAvD_BwE&gclsrc=aw.ds) connected to the LEDs.  

I've had help of a few libraries on GitHub, but most of the reverse engineering described here results out of a Packet Trace app on my Android phone and Wireshark.

## Discovery
The MagicHome app is able to discover controllers on the network without them linked to your account. That means, when we figure out how that works, we can smuggle in a fake controller!

The discovery process is obvious: the app sends a UDP broadcast (`255.255.255.255`) under the port 48899. The broadcast contains a message: `HF-A11ASSISTHREAD`. I don't know what that means, but it's the same for a lot of ZENGGE/FLUX/MagicHome devices.

Every device in the LAN network will receive this broadcast, but only controllers will understand what it means. Well, and me.

Controllers receiving this will answer with the following: `192.168.x.x,AABBCCDDEEFF,AK001-ZJ2101`. That's a comma-delimited string containing the IP address of the controller, the MAC address and the model identifier.

That string tells the MagicHome app where it can reach a controller, so we can spoof this and answer our own IP and MAC address to the UDP broadcast of the app. From now on, the communication will take place via TCP on port 5577 and in a binary protocol.

Why TCP? Because it's more reliable than UDP - and we do want that smooth scrolling over the color wheel!

## Command: Get Status
After the initial discovery has been made, the MagicHome app will ask the controller (and now us because we pretend to be one) about some basic info.

To get that status, it sends us the following: `81 8A 8B 96`, which is robot-speech for "hey, tell me about yourself!"

The controller answers with a 14-byte long payload:   
|00|01|02|03|04|05|06|07|08|09|10|11|12|13|
|--|--|--|--|--|--|--|--|--|--|--|--|--|--|
|`81`|`33`|`23`|`64`|`23`|`09`|`FF`|`FF`|`FF`|`00`|`08`|`00`|`00`|`??`|

Here's the explanation for that which I found out:

|Num|Description|
|---|-----------|
|00|The command ID, `0x81` in most cases|
|01|Model version, displayed in the app, `0x33` seems recent|
|02|Power state, `0x23` means the controller is on, `0x24` is off|
|03|Pattern mode, `0x61` means "single color", there are more|
|04|Not sure|
|05|Pattern speed, unused for single color|
|06|R color|
|07|G color|
|08|B color|
|09|W color (if the strip is RGBW)|
|10|Firmware version, `0x08` seems recent|
|11|Not sure|
|12|Not sure|
|13|Checksum|

This tells the MagicHome app about the model and firmware version, current color and whether it's switched on or off. We can simply return our own variables here, such as the color and power state, and the MagicHome app will display it like that.

## Checksum
As you've seen above, there is a checksum in the last byte of the response. The checksum is appeneded to every payload and the app uses that to verify the integrity of the data.

I was lucky to have a data set which showed, that the checksum is acting linear. Here's an example, I had two payloads:   
`F0 71 24 85` and `F0 71 23 84`. We know that the last byte is the checksum, and see how the checksum byte is decreased by one where the third byte is also decreased by one and the first two bytes are the same.

That suggests that there has to be a simple mathematical operation. Taking a binary calculator revealed their simple checksum algorithm:

All bytes except the checksum byte (which we don't know yet) are summed up: `F0+71+24 = 185 in hex`. Then, the last two digits are taken off the result: `185 => 85`.
Let's try that for the other payload too: `F0+71+23 = 184 in hex`, last two digits are `84`. So thats how they calculate their checksum. Check [Utilities.cs](https://github.com/iUltimateLP/MagicHomeController/blob/master/MagicHomeController/Utilities.cs#L53) to see how that was implemented.

## Command: Switch on / off
Now that the app is displaying our fake controller, the app can also send commands. Here's the command for switching on the controller:

|00|01|02|03|
|--|--|--|--|
|`71`|`23`|`0F`|`??`|

|Num|Description|
|---|-----------|
|00|The command ID, `0x71` it is|
|01|Whether to switch the controller on (`0x23`) or off (`0x24`)|
|02|Whether the command was sent locally (LAN) (`0x0f`) or remotely (`0xf0`)|
|03|Checksum|

So all we have to do is detect that command and look at the second byte to see what the new state should be. The response from the controller will be:

|00|01|02|03|
|--|--|--|--|
|`F0`|`71`|`23`|`??`|

|Num|Description|
|---|-----------|
|00|The inverse of the third byte above, so `0x0f` becomes `0xf0`|
|01|The command ID `0x71`|
|02|Whether we switched the controller on (`0x23`) or off (`0x24`)|
|03|Checksum|

Sending that back will let the MagicHome app know that we processed the request.

## Command: Set color
The last command I reverse engineered is the command the app sends to our controller to change the color:

|00|01|02|03|04|05|
|--|--|--|--|--|--|
|`31`|`FF`|`00`|`00`|`00`|`??`|

|Num|Description|
|---|-----------|
|00|The command ID, it's `0x31`|
|01|New R color|
|02|New G color|
|03|New B color|
|04|New W color|
|05|Checksum|

This command is easy, we don't have to send any response back, only thing we can do is parse the color bytes and handle it how we want it.
