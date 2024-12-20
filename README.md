# Unity Voxel
Adds "native" support to MagicaVoxel ".vox" files by creating models / textures for use on import.

Has methods to partially build meshes, useful for dynamic build systems and destroying parts of models.

### Forked from MagicaVoxel-Unity-Importer

[https://github.com/korobetski/MagicaVoxel-Unity-Importer](https://github.com/korobetski/MagicaVoxel-Unity-Importer)

Changes
 - Converted to Unity mathematics for Burst Support
 - Added Job to build Mesh
 - Added Base VoxelObject Class
 - Added VoxelAsset to read voxels from data
 - Meshbuilding can partially build object
 - Added Basic Voxel Building System