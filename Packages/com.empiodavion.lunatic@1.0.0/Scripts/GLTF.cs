using System.Collections.Generic;

[System.Serializable]
public class GLTF
{
	#region Classes

	[System.Serializable]
	public class Accessor
	{
		[System.Serializable]
		public class Sparse
		{
			[System.Serializable]
			public class Indices
			{
				public int bufferView;
				public int byteOffset;
				public int componentType;
			}

			[System.Serializable]
			public class Values
			{
				public int bufferView;
				public int byteOffset;
			}

			public int count;
			public Indices indices;
			public Values values;
		}

		public int bufferView;
		public int byteOffset;
		public int componentType;
		public bool normalized;
		public int count;
		public string type;
		public float[] max;
		public float[] min;
		public Sparse sparse;
	}

	[System.Serializable]
	public class Animation
	{
		[System.Serializable]
		public class Channel
		{
			[System.Serializable]
			public class Target
			{
				public int node;
				public string path;
			}

			public int sampler;
			public Target target;
		}

		[System.Serializable]
		public class Sampler
		{
			public int input;
			public string interpolation;
			public int output;
		}

		public Channel[] channels;
		public Sampler[] samplers;
		public string name;
	}

	[System.Serializable]
	public class Asset
	{
		public string copyright;
		public string generator;
		public string version;
		public string minVersion;
	}

	[System.Serializable]
	public class Buffer
	{
		public string uri;
		public int byteLength;
		public string name;
	}

	[System.Serializable]
	public class BufferView
	{
		public int buffer;
		public int byteOffset;
		public int byteLength;
		public int byteStride;
		public int target;
		public string name;
	}

	[System.Serializable]
	public class Camera
	{
		[System.Serializable]
		public class Orthographic
		{
			public float xmag;
			public float ymag;
			public float zfar;
			public float znear;
		}

		[System.Serializable]
		public class Perspective
		{
			public float aspectRatio;
			public float yfov;
			public float zfar;
			public float znear;
		}

		public Orthographic orthographic;
		public Perspective perspective;
		public string type;
		public string name;
	}

	[System.Serializable]
	public class Image
	{
		public string uri;
		public string mimeType;
		public int bufferView;
		public string name;
	}

	[System.Serializable]
	public class Material
	{
		[System.Serializable]
		public class NormalTextureInfo
		{
			public int index;
			public int texCoord;
			public float scale = 1.0f;
		}

		[System.Serializable]
		public class OcclusionTextureInfo
		{
			public int index;
			public int texCoord;
			public float strength = 1.0f;
		}

		[System.Serializable]
		public class PBRMetallicRoughness
		{
			public float[] baseColorFactor;
			public TextureInfo baseColorTexture;
			public float metallicFactor = 1.0f;
			public float roughnessFactor = 1.0f;
			public TextureInfo metallicRoughnessTexture;
		}

		public string name;
		public PBRMetallicRoughness pbrMetallicRoughness;
		public NormalTextureInfo normalTexture;
		public OcclusionTextureInfo occlusionTexture;
		public TextureInfo emissiveTexture;
		public float[] emissiveFactor;
		public string alphaMode = "OPAQUE";
		public float alphaCutoff = 0.5f;
		public bool doubleSided;
	}

	[System.Serializable]
	public class Mesh
	{
		[System.Serializable]
		public class Primitive
		{
			[System.Serializable]
			public class Attributes
			{
				public int NORMAL = -1;
				public int POSITION = -1;
				public int TANGENT = -1;
				public int TEXCOORD_0 = -1;
				public int COLOR_0 = -1;
				public int JOINTS_0 = -1;
				public int WEIGHTS_0 = -1;
			}

			[System.Serializable]
			public class Targets
			{
				public int NORMAL = -1;
				public int POSITION = -1;
				public int TANGENT = -1;
			}

			public Attributes attributes;
			public int indices;
			public int material;
			public int mode = 4;
			public Targets targets;
		}

		public Primitive[] primitives;
		public float[] weights;
		public string name;
	}

	[System.Serializable]
	public class Node
	{
		public int camera = -1;
		public int[] children;
		public int skin = -1;
		public float[] matrix;
		public int mesh = -1;
		public float[] rotation;
		public float[] scale;
		public float[] translation;
		public float[] weights;
		public string name;
	}

	[System.Serializable]
	public class Sampler
	{
		public int magFilter;
		public int minFilter;
		public int wrapS = 10497;
		public int wrapT = 10497;
		public string name;
	}

	[System.Serializable]
	public class Scene
	{
		public int[] nodes;
		public string name;
	}

	[System.Serializable]
	public class Skin
	{
		public int inverseBindMatrices;
		public int skeleton;
		public int[] joints;
		public string name;
	}

	[System.Serializable]
	public class Texture
	{
		public int sampler;
		public int source;
		public string name;
	}

	[System.Serializable]
	public class TextureInfo
	{
		public int index;
		public int texCoord;
	}

	#endregion

	private string path;

	public string[] extensionsUsed;
	public string[] extensionsRequired;
	public Accessor[] accessors;
	public Animation[] animations;
	public Asset asset;
	public Buffer[] buffers;
	public BufferView[] bufferViews;
	public Camera[] cameras;
	public Image[] images;
	public Material[] materials;
	public Mesh[] meshes;
	public Node[] nodes;
	public Sampler[] samplers;
	public int scene;
	public Scene[] scenes;
	public Skin[] skins;
	public Texture[] textures;

#if UNITY_EDITOR

	[UnityEditor.MenuItem("Assets/Load GLTF")]
	private static void TestLoad()
	{
		string path = UnityEditor.EditorUtility.OpenFilePanel("Open GLTF", "", "gltf");

		if (!string.IsNullOrEmpty(path))
		{
			GLTF gltf = LoadGLTF(path);

			gltf.ConvertToGameObject();
		}
	}

#endif

	public static GLTF LoadGLTF(string path)
	{
		string json = System.IO.File.ReadAllText(path);

		GLTF gltf = Newtonsoft.Json.JsonConvert.DeserializeObject<GLTF>(json);
		gltf.path = path;

		return gltf;
	}

	private byte[][] LoadBuffers()
	{
		byte[][] bufferDatas = new byte[buffers.Length][];

		for (int i = 0; i < buffers.Length; i++)
		{
			byte[] data = LoadURI(buffers[i].uri);

			if (data != null)
				bufferDatas[i] = data;
			else
				return null;
		}

		return bufferDatas;
	}

	private UnityEngine.Mesh[] LoadMeshes(byte[][] bufferDatas)
	{
		UnityEngine.Mesh[] umeshes = FillArray<UnityEngine.Mesh>(meshes.Length);

		for (int i = 0; i < meshes.Length; i++)
		{
			Mesh mesh = meshes[i];
			UnityEngine.Mesh umesh = umeshes[i];

			if (mesh.name != null)
				umesh.name = mesh.name;

			if (mesh.primitives != null)
			{
				umesh.subMeshCount = mesh.primitives.Length;

				List<int[]> indexList = new List<int[]>(mesh.primitives.Length);
				List<UnityEngine.Vector3> vertexList = new List<UnityEngine.Vector3>();
				List<UnityEngine.Vector3> normalList = new List<UnityEngine.Vector3>();
				List<UnityEngine.Vector4> tangentList = new List<UnityEngine.Vector4>();
				List<UnityEngine.Vector2> uvList = new List<UnityEngine.Vector2>();
				List<UnityEngine.MeshTopology> topologyList = new List<UnityEngine.MeshTopology>(mesh.primitives.Length);

				// load vertices and submeshes
				for (int j = 0; j < mesh.primitives.Length; j++)
				{
					Mesh.Primitive primitive = mesh.primitives[j];

					UnityEngine.MeshTopology topology = primitive.mode switch
					{
						0 => UnityEngine.MeshTopology.Points,
						1 => UnityEngine.MeshTopology.Lines,
						// 2 => line loop
						3 => UnityEngine.MeshTopology.LineStrip,
						4 => UnityEngine.MeshTopology.Triangles,
						// 5 => triangle strip
						// 6 => triangle fan
						_ => UnityEngine.MeshTopology.Triangles
					};

					int id = indexList.Count;
					indexList.Add(LoadIntData(primitive.indices, bufferDatas));

					if (id > 0)
					{
						for (int k = 0; k < indexList[id].Length; k++)
							indexList[id][k] += vertexList.Count;
					}

					vertexList.AddRange(LoadVector3Data(primitive.attributes.POSITION, bufferDatas));
					normalList.AddRange(LoadVector3Data(primitive.attributes.NORMAL, bufferDatas));
					tangentList.AddRange(LoadVector4Data(primitive.attributes.TANGENT, bufferDatas));
					uvList.AddRange(LoadVector2Data(primitive.attributes.TEXCOORD_0, bufferDatas));

					//primitive.attributes.COLOR_0
					//primitive.attributes.WEIGHTS_0
					//primitive.attributes.JOINTS_0

					topologyList.Add(topology);
				}

				umesh.SetVertices(vertexList);
				umesh.SetNormals(normalList);
				umesh.SetTangents(tangentList);
				umesh.SetUVs(0, uvList);

				for (int j = 0; j < indexList.Count; j++)
					umesh.SetIndices(indexList[j], topologyList[j], j);

				if (normalList.Count == 0)
					umesh.RecalculateNormals();

				if (tangentList.Count == 0)
					umesh.RecalculateTangents();
			}
		}

		return umeshes;
	}

	private int GetComponentCount(string type)
	{
		return type switch
		{
			"SCALAR" => 1,
			"VEC2" => 2,
			"VEC3" => 3,
			"VEC4" => 4,
			"MAT2" => 4,
			"MAT3" => 9,
			"MAT4" => 16,
			_ => 1
		};
	}

	private int GetComponentSize(int type)
	{
		return type switch
		{
			5120 => 1, // byte
			5121 => 1, // unsigned byte
			5122 => 2, // short
			5123 => 2, // unsigned short
			5125 => 4, // unsigned int
			5126 => 4, // float
			_ => 1
		};
	}

	private UnityEngine.Vector2[] LoadVector2Data(int accessor, byte[][] bufferDatas) =>
		LoadFloatData(accessor, bufferDatas, 2, ToVector2);

	private UnityEngine.Vector3[] LoadVector3Data(int accessor, byte[][] bufferDatas) =>
		LoadFloatData(accessor, bufferDatas, 3, ToVector3);

	private UnityEngine.Vector4[] LoadVector4Data(int accessor, byte[][] bufferDatas) =>
		LoadFloatData(accessor, bufferDatas, 4, ToVector4);

	private System.Func<float> GetReadFloat(System.IO.BinaryReader reader, int type)
	{
		return type switch
		{
			5120 => () => reader.ReadSByte(),
			5121 => () => reader.ReadByte(),
			5122 => () => reader.ReadInt16(),
			5123 => () => reader.ReadUInt16(),
			5125 => () => reader.ReadUInt32(),
			5126 => () => reader.ReadSingle(),
			_ => () => reader.ReadSingle()
		};
	}

	private System.Func<int> GetReadInt(System.IO.BinaryReader reader, int type)
	{
		return type switch
		{
			5120 => () => reader.ReadSByte(),
			5121 => () => reader.ReadByte(),
			5122 => () => reader.ReadInt16(),
			5123 => () => reader.ReadUInt16(),
			5125 => () => (int)reader.ReadUInt32(),
			5126 => () => (int)reader.ReadSingle(),
			_ => () => reader.ReadInt32()
		};
	}

	private T[] LoadFloatData<T>(int accessorIndex, byte[][] bufferDatas, int components,
		System.Func<float[], T> create)
	{
		if (accessorIndex < 0)
			return new T[0];

		Accessor accessor = accessors[accessorIndex];

		if (accessor.sparse != null)
		{
			UnityEngine.Debug.LogWarning("Support for sparse accessors not implemented.");
			return null;
		}
		else
		{
			BufferView bufferView = bufferViews[accessor.bufferView];
			byte[] bufferData = bufferDatas[bufferView.buffer];

			using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bufferData))
			using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
			{
				return LoadTypeData(accessor, reader, components, GetReadFloat, create);
			}
		}
	}

	private int[] LoadIntData(int accessorIndex, byte[][] bufferDatas)
	{
		if (accessorIndex < 0)
			return new int[0];

		Accessor accessor = accessors[accessorIndex];

		if (accessor.sparse != null)
		{
			UnityEngine.Debug.LogWarning("Support for sparse accessors not implemented.");
			return null;
		}
		else
		{
			BufferView bufferView = bufferViews[accessor.bufferView];
			byte[] bufferData = bufferDatas[bufferView.buffer];

			using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bufferData))
			using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
			{
				return LoadTypeData(accessor, reader, 1, GetReadInt, (t) => t[0]);
			}
		}
	}

	private T[] LoadTypeData<T, U>(Accessor accessor, System.IO.BinaryReader reader, int components,
		System.Func<System.IO.BinaryReader, int, System.Func<U>> readFunc, System.Func<U[], T> create)
	{
		BufferView bufferView = bufferViews[accessor.bufferView];
		int pos = bufferView.byteOffset + accessor.byteOffset;
				reader.BaseStream.Seek(pos, System.IO.SeekOrigin.Begin);

		int count = GetComponentCount(accessor.type);
		int size = GetComponentSize(accessor.componentType);

		T[] vecs = new T[accessor.count];
		U[] temp = new U[UnityEngine.Mathf.Max(count, components)];

		int stride = count * size;
		int offset = bufferView.byteStride - stride;
		bool needsOffset = offset > 0;
		System.Func<U> read = readFunc(reader, accessor.componentType);

		for (int i = 0; i < accessor.count; i++)
		{
			for (int j = 0; j < count; j++)
			{
				temp[j] = read();
			}

			if (needsOffset)
				reader.BaseStream.Seek(offset, System.IO.SeekOrigin.Current);

			vecs[i] = create(temp);
		}

		return vecs;
	}

	private UnityEngine.Texture2D[] LoadTextures(byte[][] bufferDatas)
	{
		UnityEngine.Texture2D[] utextures = FillArray(images.Length, () => new UnityEngine.Texture2D(1, 1));

		for (int i = 0; i < images.Length; i++)
		{
			Image image = images[i];
			UnityEngine.Texture2D utexture = utextures[i];

			utexture.name = image.name;

			if (image.uri != null)
			{
				byte[] data = LoadURI(image.uri);
				UnityEngine.ImageConversion.LoadImage(utexture, data);
			}
			else
			{
				// load from buffer view
				BufferView bv = bufferViews[image.bufferView];
				byte[] bufferData = bufferDatas[bv.buffer];

				if (bv.byteOffset != 0)
				{
					int len = bv.byteLength != 0 ? bv.byteLength : bufferData.Length - bv.byteOffset;
					byte[] dummy = new byte[len];

					System.Array.Copy(bufferData, bv.byteOffset, dummy, 0, len);
					bufferData = dummy;
				}

				UnityEngine.ImageConversion.LoadImage(utexture, bufferData);
			}
		}

		return utextures;
	}

	private UnityEngine.Material[] LoadMaterials(UnityEngine.Texture2D[] utextures)
	{
		UnityEngine.Shader baseShader = UnityEngine.Shader.Find("Shader Forge/Object");

		UnityEngine.Material[] umaterials = FillArray(materials.Length, () => new UnityEngine.Material(baseShader));

		for (int i = 0; i < materials.Length; i++)
		{
			Material material = materials[i];
			UnityEngine.Material umaterial = umaterials[i];

			if (material.name != null)
				umaterial.name = material.name;

			umaterial.SetFloat("_Cutoff", material.alphaCutoff);

			if (material.emissiveTexture != null)
			{
				umaterial.SetTexture("_Emission", utextures[material.emissiveTexture.index]);
				// assign to uv set?
			}
			else
				umaterial.SetTexture("_Emission", UnityEngine.Texture2D.blackTexture);

			if (material.pbrMetallicRoughness != null)
			{
				Material.PBRMetallicRoughness pbr = material.pbrMetallicRoughness;

				umaterial.SetFloat("_Shine", 1.0f - pbr.roughnessFactor);

				if (pbr.baseColorTexture != null)
				{
					Texture tex = textures[pbr.baseColorTexture.index];
					UnityEngine.Texture2D utex = utextures[tex.source];

					if (samplers != null)
					{
						Sampler sampler = samplers[tex.sampler];
						utex.wrapModeU = GetWrapMode(sampler.wrapS);
						utex.wrapModeV = GetWrapMode(sampler.wrapT);
					}

					umaterial.SetTexture("_MainTex", utextures[tex.source]);

					// uvs seem to be vertically flipped, or texture may be importing flipped
					umaterial.SetTextureScale("_MainTex", new UnityEngine.Vector2(1.0f, -1.0f));

					// game shader has no property for colour/tint
					if (pbr.baseColorFactor != null)
						umaterial.SetColor("_Color", ToVector4(pbr.baseColorFactor));
				}
			}
		}

		return umaterials;
	}

	private UnityEngine.Transform[] LoadNodes(UnityEngine.Mesh[] umeshes, UnityEngine.Material[] umaterials)
	{ 
		UnityEngine.GameObject[] gameObjects = FillArray<UnityEngine.GameObject>(nodes.Length);
		UnityEngine.Transform[] transforms = System.Array.ConvertAll(gameObjects, (x) => x.transform);

		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].name != null)
				gameObjects[i].name = nodes[i].name;

			if (nodes[i].matrix != null)
			{
				UnityEngine.Matrix4x4 mat = ToMatrix(nodes[i].matrix);
				transforms[i].SetPositionAndRotation(mat.GetRow(3), mat.rotation);
				transforms[i].localScale = mat.lossyScale;
			}
			else
			{
				if (nodes[i].translation != null)
					transforms[i].localPosition = ToVector3(nodes[i].translation);

				if (nodes[i].rotation != null)
					transforms[i].localRotation = ToQuaternion(nodes[i].rotation);

				if (nodes[i].scale != null)
					transforms[i].localScale = ToVector3(nodes[i].scale);
			}

			if (nodes[i].mesh > -1)
			{
				int m = nodes[i].mesh;

				UnityEngine.MeshFilter filter = gameObjects[i].AddComponent<UnityEngine.MeshFilter>();
				filter.sharedMesh = umeshes[m];

				if (meshes[m].name != null)
					umeshes[m].name = meshes[m].name;

				if (meshes[m].primitives != null)
				{
					// if has weights/skin, add SkinnedMeshRenderer instead

					UnityEngine.MeshRenderer meshRenderer = gameObjects[i].AddComponent<UnityEngine.MeshRenderer>();

					UnityEngine.Material[] mats = new UnityEngine.Material[meshes[m].primitives.Length];

					for (int j = 0; j < meshes[m].primitives.Length; j++)
						mats[j] = umaterials[meshes[m].primitives[j].material];

					meshRenderer.sharedMaterials = mats;
				}
			}

			if (nodes[i].children != null)
			{
				for (int j = 0; j < nodes[i].children.Length; j++)
					transforms[nodes[i].children[j]].SetParent(transforms[i], true);
			}
		}

		return transforms;
	}

	private UnityEngine.FilterMode GetFilterMode(int filter)
	{
		return filter switch
		{
			9728 => UnityEngine.FilterMode.Point,		// nearest
			9729 => UnityEngine.FilterMode.Bilinear,	// linear
			9984 => UnityEngine.FilterMode.Bilinear,	// nearest_mipmap_nearest
			9985 => UnityEngine.FilterMode.Bilinear,	// linear_mipmap_nearest
			9986 => UnityEngine.FilterMode.Bilinear,	// nearest_mipmap_linear
			9987 => UnityEngine.FilterMode.Trilinear,	// linear_mipmap_linear
			_ => UnityEngine.FilterMode.Bilinear
		};
	}

	private UnityEngine.TextureWrapMode GetWrapMode(int wrap)
	{
		return wrap switch
		{
			10497 => UnityEngine.TextureWrapMode.Repeat,
			33071 => UnityEngine.TextureWrapMode.Clamp,
			33648 => UnityEngine.TextureWrapMode.Mirror,
			_ => UnityEngine.TextureWrapMode.Repeat
		};
	}

	public UnityEngine.GameObject ConvertToGameObject()
	{
		byte[][] bufferDatas = LoadBuffers();

		if (bufferDatas == null)
			return null;

		UnityEngine.Mesh[] umeshes = LoadMeshes(bufferDatas);
		UnityEngine.Texture2D[] utextures = LoadTextures(bufferDatas);
		UnityEngine.Material[] umaterials = LoadMaterials(utextures);
		UnityEngine.Transform[] transforms = LoadNodes(umeshes, umaterials);

		UnityEngine.GameObject root = new UnityEngine.GameObject("Root");
		UnityEngine.Transform rootTransform = root.transform;

		foreach (UnityEngine.Transform transform in transforms)
			if (transform.parent == null)
				transform.SetParent(rootTransform, true);

		// gltf files use -x as the right axis, unity uses +x as the right axis
		rootTransform.localScale = new UnityEngine.Vector3(-1.0f, 1.0f, 1.0f);
		return rootTransform.gameObject;
	}

	private static UnityEngine.Vector2 ToVector2(float[] floats) =>
		new UnityEngine.Vector2(floats[0], floats[1]);

	private static UnityEngine.Vector3 ToVector3(float[] floats) =>
		new UnityEngine.Vector3(floats[0], floats[1], floats[2]);

	private static UnityEngine.Vector4 ToVector4(float[] floats) =>
		new UnityEngine.Vector4(floats[0], floats[1], floats[2], floats[3]);

	private static UnityEngine.Quaternion ToQuaternion(float[] floats) =>
		new UnityEngine.Quaternion(floats[0], floats[1], floats[2], floats[3]);

	private static UnityEngine.Matrix4x4 ToMatrix(float[] floats) =>
		new UnityEngine.Matrix4x4(
			new UnityEngine.Vector4(floats[0], floats[1], floats[2], floats[3]),
			new UnityEngine.Vector4(floats[4], floats[5], floats[6], floats[7]),
			new UnityEngine.Vector4(floats[8], floats[9], floats[10], floats[11]),
			new UnityEngine.Vector4(floats[12], floats[13], floats[14], floats[15]));

	private T[] FillArray<T>(int len) where T : new() => FillArray<T>(len, () => new T());

	private T[] FillArray<T>(int len, System.Func<T> create)
	{
		T[] array = new T[len];

		for (int i = 0; i < len; i++)
			array[i] = create();

		return array;
	}

	private byte[] LoadURI(string uri)
	{
		byte[] data;

		if (uri.StartsWith("data:"))
		{
			// decode
			int comma = uri.IndexOf(',');

			int isBase64 = uri.IndexOf("base64", 0, comma);

			if (isBase64 >= 0)
			{
				uri = uri.Substring(comma + 1);
				data = System.Convert.FromBase64String(uri);
			}
			else
			{
				string type = uri.Substring(0, comma);
				UnityEngine.Debug.LogWarning($"Reading data URI of {type} is not implemented.");
				return null;
			}
		}
		else
		{
			string loc = System.IO.Path.GetDirectoryName(path);
			loc = System.IO.Path.Combine(loc, uri);

			if (!System.IO.File.Exists(loc))
			{
				UnityEngine.Debug.LogWarning("Could not find file at " + loc);
				return null;
			}

			data = System.IO.File.ReadAllBytes(loc);
		}

		return data;
	}
}
