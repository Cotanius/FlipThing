# FlipThing
 Shader based effect for Unity inspired by Flipdots. Made in Built-in Render Pipeline. Unity-chan asset was used.

![Screen](https://github.com/Cotanius/FlipThing/blob/main/Media/FlipThing1.png)
![GIF](https://github.com/Cotanius/FlipThing/blob/main/Media/Flipthing.gif)

## Breakdown
There is a modular prefab that includes a camera and a MeshRenderer. It generates an array mesh on awake from a given pixel mesh and a few parameters. The mesh has it's texcoord1 property used for indexing the pixels for them to be alligned to a RenderTexture that is filled by the camera. It's color property is used for the local coordinates of each pixel mesh. Animation is done by introducing a Time.deltaTime operation while writing from the camera's target RenderTexture to a Texture2D that is assigned to the shader.

## Usage

The panels are made from arrays of the prefab. You can optimize it by maximizing the 65k vertices limit on the meshes by adjusting the parameters and changing your input pixel mesh vertex count. There can be further optimization done by sharing meshes among identical prefab instances but I haven't got to that yet.
