
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSurfaceLibrary : MonoBehaviour
{
    public List<TerrainSurface> terrainSurfaces = new List<TerrainSurface>();
	[Space]
    public TerrainLayer[] terrainLayers = new TerrainLayer[0];
    public Terrain terrain;

	public string GetSurfaceTag(Vector3 position)
    {
		int layerID = GetMainTexture(position, terrain);
		if (layerID >= terrainSurfaces.Count) return string.Empty;

		return terrainSurfaces[layerID].Tag;
    }

	public static float[] GetTextureMix(Vector3 worldPos, Terrain terrain)
	{
		// 返回包含各贴图相对混合权重的数组
		// 表示主地形在该世界坐标处的混合情况。
		// 数组中的值数量等于
		// 地形上添加的贴图数量。
		TerrainData terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;

		// 计算 worldPos 落在哪个 splat map 单元（忽略 y）
		int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

		// 以 1x1xN 的 3D 数组取出该单元的 splat 数据（N = 贴图数量）
		float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

		// 把 3D 数组数据展开到 1D 数组：
		float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

		for (int n = 0; n < cellMix.Length; ++n)
		{
			cellMix[n] = splatmapData[0, 0, n];
		}

		return cellMix;
	}

	public static int GetMainTexture(Vector3 worldPos, Terrain terrain)
	{
		// 返回占主导的贴图的从 0 开始的索引
		// 表示主地形在该世界坐标处的混合情况。
		float[] mix = GetTextureMix(worldPos, terrain);
		float maxMix = 0;
		int maxIndex = 0;

		// 遍历每个混合值并找出最大值
		for (int n = 0; n < mix.Length; ++n)
		{
			if (mix[n] > maxMix)
			{
				maxIndex = n;
				maxMix = mix[n];
			}
		}

		return maxIndex;
	}


	private void OnValidate()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        if (terrain == null) return;

		terrainLayers = terrain.terrainData.terrainLayers;
        for (int i = 0; i < terrainLayers.Length; i++)
        {
            if (!terrainSurfaces.Exists(x => x.terrainLayer == terrainLayers[i]))
            {
                terrainSurfaces.Add(new TerrainSurface()
                {
                    terrainLayer = terrainLayers[i],
                    MainTexture = terrainLayers[i].diffuseTexture,
                    Tag = "Generic",
                });
            }
        }
    }

    [System.Serializable]
    public class TerrainSurface
    {
        public string Tag;
        [SpritePreview] public Texture2D MainTexture;
        public TerrainLayer terrainLayer;
    }
}