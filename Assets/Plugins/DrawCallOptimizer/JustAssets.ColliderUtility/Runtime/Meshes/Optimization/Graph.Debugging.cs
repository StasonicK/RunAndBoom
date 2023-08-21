//#define STEP_BY_STEP_EXECUTION

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public partial class Graph
    {
#if STEP_BY_STEP_EXECUTION
        public Graph(string hexContent) : this(0.0001f)
        {
            var data = StringToByteArray(hexContent);
            using var fileStream = new MemoryStream(data);
            using var reader = new BinaryReader(fileStream);
            Deserialize(reader, out var indices, out var vertices);

            CreateData(null, vertices, indices);
        }

        public static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        

        private static void Deserialize(BinaryReader reader, out List<List<int>> indices, out Vector3[] vertices)
        {
            var subMeshCount = reader.ReadInt32();
            indices = new List<List<int>>(subMeshCount);
            for (var smi = 0; smi < subMeshCount; smi++)
            {
                var indexCount = reader.ReadInt32();
                var subMeshes = new List<int>(indexCount);
                for (var i = 0; i < indexCount; i++)
                    subMeshes.Add(reader.ReadInt32());
                indices.Add(subMeshes);
            }

            vertices = new Vector3[reader.ReadInt32()];
            for (var i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
#endif

        [Conditional("STEP_BY_STEP_EXECUTION")]
        private static void SerializeToFile(List<List<int>> ind, Vector3[] meshVertices)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    Serialize(writer, ind, meshVertices);
                    writer.Flush();
                    File.WriteAllText("d:\\meshdata.dat", ByteArrayToString(memoryStream.ToArray()));
                }
            }
        }

        [Conditional("STEP_BY_STEP_EXECUTION")]
        private static void Serialize(BinaryWriter writer, List<List<int>> indices, Vector3[] vertices)
        {
            writer.Write(indices.Count);
            foreach (var indexList in indices)
            {
                writer.Write(indexList.Count);
                foreach (var i in indexList)
                    writer.Write(i);
            }
            writer.Write(vertices.Length);
            foreach (var vector3 in vertices)
            {
                writer.Write(vector3.x);
                writer.Write(vector3.y);
                writer.Write(vector3.z);
            }
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
