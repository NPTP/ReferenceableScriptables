# Referenceable Scriptables

The Addressables package has a limitation when used in conjunction with a Scriptable Object (SO) in Unity.

If an SO is loaded via Addressables, the SO and any references to other assets inside are baked into the Addressables asset library, such that they are effectively copies of the original assets. At runtime, this means that the same SO referenced directly in a scene and that loaded by Addressables are actually two separate instances.

One solution is to load all SOs via Addressables. But if you can't do that, or don't want to do that, your options are a bit limited. Enter this package, which allows you to treat SOs like Addressables, putting them anywhere in your project, checking a box and loading them by a key, leveraging the Resources folder automatically to ensure any scene/prefab references to an SO refer to the same instance and you get the results your expect.