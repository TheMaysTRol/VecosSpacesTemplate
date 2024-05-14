## Vecos Spaces Template
Welcome to the official repository for Vecos Metaverse Spaces Creator.

Vecos is a metaverse creator application available on our [website](https://hyper.vecos.us). This repository hosts a Unity template designed to enable you to create Vecos spaces (metaverses) within the Vecos metaverse itself.

At present, this project is using Unity version 2022.3.16f1.

----------------------------------------
## Quick Start
- You will have to clone this repository.
```git
git clone https://github.com/TheMaysTRol/VecosSpacesTemplate.git
```
- Open the project using unity hub.
- Start creating your own scene (follow for more specifications)

## Setup the scene
- Let's start by creating a new scene in the Scenes folder,You can name the scene as you like(in my case I choose the name Demo).

![](https://i.imgur.com/VaRh8NS.png)

 Lets add the created scene to the unity addressables system :
  Go to Window -> Asset Management -> Addressables -> Groups
In the groups window you will find an exisiting default group called "AddressablesGroup"

![](https://i.imgur.com/0VIYMGO.png)

 Grab the scene you've just created and drop it inside that group

![](https://i.imgur.com/a3SemXv.png)

 Right click on the scene in the addressables groups window and choose "Simplify Addressables Names"

![](https://i.imgur.com/1lMD91A.png)

## Create the scene that you want

- You can now add objects to your scene(3d models,ui ...)

Example scene : 

![](https://i.imgur.com/nvExiHr.png)


## Spawn a test player in the scene

 To spawn a player in the scene for testing purposes, follow these steps:
 
 - Search for 'Vecos' in the toolbar.
   ![](https://i.imgur.com/C4RVNqH.png)
   
 - Navigate to Vecos -> Testing Scene.
   
   ![](https://i.imgur.com/jBdNO5A.png)
   
 - If you're testing on a PC or mobile device, click on 'Create PC Player.
   ![](https://i.imgur.com/Iwk3hwE.png)
   
 - If you're testing on a VR device, click on 'Create VR Player.
   ![](https://i.imgur.com/4t3EtPw.png)
