# Referenceable Scriptables

The Addressables package has a limitation when used in conjunction with a Scriptable Object (SO) in Unity.

Addressable SOs get baked into their own asset library. Let's say we have an SO named "MyAsset".

If we reference "MyAsset" in a serialized field of type `ScriptableObject`, we'll refer to it as "MyAsset-A".
The version of the asset in the Addressables library is a copy, which we'll refer to as "MyAsset-B".

If we check "MyAsset-A" == "MyAsset-B" in code, we will get false, even though we thought we were checking the same asset!

Enter this package, which allows you to treat SOs like Addressables, putting them anywhere in your project, checking a box and loading them in and out as required, but when you load "MyAsset", you are always getting "MyAsset-A" - the same one you expect when you reference "MyAsset" directly in a serialized field.

Under the hood this automatically leverages Unity's Resources system without being forced to use Resources in the usual clunky way (fixed paths, fixed data locations, etc).

Just check the box marked "Referenceable" at the top of the inspector on any `ScriptableObject`, or use the menu at Tools/Referenceables Management to modify this setting in aggregate on scriptable objects in your project.