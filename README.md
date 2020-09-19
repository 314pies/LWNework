# LWNet

LWNetwork is a light weight high level multiplayer framework that has a very good integration with the Unity game engine. It has also been used by our own popular mobile multiplayer shooter game - ["Local Warfare: Name Uknown"](https://play.google.com/store/apps/details?id=com.BUProduct.LocalWarfarePortable). 

### Note: We are still working on the documantations and example projects. 

## Showcase (Local Warfare: Name Uknown)

<iframe width="560" height="315" src="https://www.youtube.com/embed/rTJNELqfzIw" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Architecture

LWNetwork containts the following main components:

* **Network Objects Manager** to manage netowork objects.
* **Network View** to manage network object id, states and RPC methods.
* **Syn Variables** to synchronize states on network object.
* **[RPC]** to invoke remote functions across network objects.
* **Network Events** for handling remote peers connection events 

## Support Plarforms
For now, LWNetwork support the following platforms:
* Windows
* Mac
* Android
* iOS

### License (MIT)
[See this link for more information](https://github.com/314pies/LWNework/blob/master/LICENSE)
