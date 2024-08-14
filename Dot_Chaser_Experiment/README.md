# Dot Chaser README

Last updated: `2024-06-06`
## TO DO BEFORE I CAN DO ANYTHING ELSE

I need to set up the VR Rig assets, including eye gaze bits, to be prefabs of my own. Currently, it feels like they're too fragile to edit/change myself (which Unity warns you about and I knew going in)

So, the next step will be copying their building block assets and unlocking them from Meta's guard rails. This *should* be doable.

## Dot Chaser Readme

I (Trent) wanted to provide a short list of things required to get this project up and running, so that if/when we go to make a new one we can follow all of the same steps:

### For Rapid Dev with Playmode in Unity
- Meta all-in-one SDK installed in Unity (can be done from the assets store)
- android developer bridge (adb) must be installed on the PC
- adb must be added to path
- Headset (quest, whatever) needs to be recognizeable by the terminal command `adb devices`; this way the headset can activate it
- Meta Quest Link app needs to be installed on PC
- In the Meta Quest Link app, OpenXR Runtime needs to be set as active
- (I'm not sure if this was necessary, but I did it): the target build device needs to be Android
- in `Edit > Project Settings > XR Plug-in Management`, Oculus (if you're using a Meta Quest Pro like I am) should be checked as the selected as the `Plug-in Provider` for **BOTH** Android and Windows/Mac/Linux
- In `XR Plug-in Management`, the "Initialize XR on Start-up" box should also be checked
- a VR Rig needs to be in the scene; Meta all-in-one SDK should have resulted in a little meta icon being in the bottom corner of Unity. You can add a VR Rig from the building blocks there, but be warned that you cannot alter this prefab until you make a copy of it in your own assets folder/directory

