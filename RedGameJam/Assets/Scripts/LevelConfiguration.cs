using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelConfiguration : ScriptableObject
{
    public List<Cube> Cubes;
    public List<WallChunk> WallChunks;
    public WallChunk EndChunk;
    public int Length;
    public Color PickaxeColor = Color.white;
    public Color ShovelColor = Color.white;
}
