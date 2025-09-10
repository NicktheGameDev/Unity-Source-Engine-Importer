# uSource with Blendshape (Flex) Support

This integration adds direct blendshape support from Source Engine flex data into the uSource library.

## What It Does

- Reads flex data (.mdl) without requiring .vta files.
- Applies all flexes as blendshapes in Unity.

## Installation

1. Replace your existing `uSource-master` folder with this one, or merge the `Integration` folder into your `uSource-master`.
2. Place your `.mdl`, `.vvd`, and `.vtx` files under `Assets/`.
3. In Unity, open **uSource > Import Model with Blendshapes**.
4. Select the `.mdl` TextAsset and click **Import**.

## Notes

- Tested with uSource MDL v?.? and Unity HDRP.
- Flex names are read directly from the MDL data.
