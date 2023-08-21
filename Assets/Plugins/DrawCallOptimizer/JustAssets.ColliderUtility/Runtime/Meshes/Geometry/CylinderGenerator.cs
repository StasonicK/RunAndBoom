using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Geometry
{
    public class CylinderGenerator
    {
        private const int DefaultRadialSegments = 8;

        private const int HeightSegments = 2;

        private const int MinRadialSegments = 3;

        private const int MinHeightSegments = 1;
        

        public static Mesh Create(float radius, float height, int radialSegments = DefaultRadialSegments,
            int heightSegments = HeightSegments, bool drawCaps = true)
        {
            //create the mesh
            Mesh mesh = new Mesh();

            //sanity check
            if (radialSegments < MinRadialSegments) radialSegments = MinRadialSegments;
            if (heightSegments < MinHeightSegments) heightSegments = MinHeightSegments;

            //calculate how many vertices we need
            int numVertexColumns = radialSegments + 1;
            int numVertexRows = heightSegments + 1;

            //calculate sizes
            int numVertices = numVertexColumns * numVertexRows;
            int numUVs = numVertices; //always
            int numSideTris = radialSegments * heightSegments * 2; //for one cap
            int numCapTris = drawCaps ? radialSegments - 2 : 0; 
            int trisArrayLength = (numSideTris + numCapTris * 2) * 3; //3 places in the array for each tri

            //initialize arrays
            Vector3[] vertices = new Vector3[numVertices];
            Vector2[] uVs = new Vector2[numUVs];
            int[] tris = new int[trisArrayLength];

            //precalculate increments to improve performance
            float heightStep = height / heightSegments;
            float angleStep = 2 * Mathf.PI / radialSegments;
            float uvStepH = 1.0f / radialSegments;
            float uvStepV = 1.0f / heightSegments;

            for (int j = 0; j < numVertexRows; j++)
            for (int i = 0; i < numVertexColumns; i++)
            {
                //calculate angle for that vertex on the unit circle
                float angle = i * angleStep;

                //"fold" the sheet around as a cylinder by placing the first and last vertex of each row at the same spot
                if (i == numVertexColumns - 1)
                    angle = 0;

                //position current vertex
                vertices[j * numVertexColumns + i] = new Vector3(radius * Mathf.Cos(angle), j * heightStep,
                    radius * Mathf.Sin(angle));

                //calculate UVs
                uVs[j * numVertexColumns + i] = new Vector2(i * uvStepH, j * uvStepV);

                //create the tris				
                if (j == 0 || i >= numVertexColumns - 1)
                {
                    //nothing to do on the first and last "floor" on the tris, capping is done below
                    //also nothing to do on the last column of vertices
                }
                else
                {
                    //create 2 tris below each vertex
                    //6 seems like a magic number. For every vertex we draw 2 tris in this for-loop, therefore we need 2*3=6 indices in the Tris array
                    //offset the base by the number of slots we need for the bottom cap tris. Those will be populated once we draw the cap
                    int baseIndex = numCapTris * 3 + (j - 1) * radialSegments * 6 + i * 6;

                    //1st tri - below and in front
                    tris[baseIndex + 0] = j * numVertexColumns + i;
                    tris[baseIndex + 1] = j * numVertexColumns + i + 1;
                    tris[baseIndex + 2] = (j - 1) * numVertexColumns + i;

                    //2nd tri - the one it doesn't touch
                    tris[baseIndex + 3] = (j - 1) * numVertexColumns + i;
                    tris[baseIndex + 4] = j * numVertexColumns + i + 1;
                    tris[baseIndex + 5] = (j - 1) * numVertexColumns + i + 1;
                }
            }

            if (drawCaps)
                AddCaps(numVertices, numVertexColumns, numCapTris, numSideTris, tris);

            //assign vertices, uvs and tris
            mesh.vertices = vertices;
            mesh.uv = uVs;
            mesh.triangles = tris;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddCaps(int numVertices, int numVertexColumns, int numCapTris, int numSideTris, int[] tris)
        {
            bool leftSided = true;
            int leftIndex = 0;
            int rightIndex = 0;
            int topCapVertexOffset = numVertices - numVertexColumns;
            for (int i = 0; i < numCapTris; i++)
            {
                int bottomCapBaseIndex = i * 3;
                int topCapBaseIndex = (numCapTris + numSideTris) * 3 + i * 3;

                int middleIndex;
                if (i == 0)
                {
                    middleIndex = 0;
                    leftIndex = 1;
                    rightIndex = numVertexColumns - 2;
                    leftSided = true;
                }
                else if (leftSided)
                {
                    middleIndex = rightIndex;
                    rightIndex--;
                }
                else
                {
                    middleIndex = leftIndex;
                    leftIndex++;
                }

                leftSided = !leftSided;

                //assign bottom tris
                tris[bottomCapBaseIndex + 0] = rightIndex;
                tris[bottomCapBaseIndex + 1] = middleIndex;
                tris[bottomCapBaseIndex + 2] = leftIndex;

                //assign top tris
                tris[topCapBaseIndex + 0] = topCapVertexOffset + leftIndex;
                tris[topCapBaseIndex + 1] = topCapVertexOffset + middleIndex;
                tris[topCapBaseIndex + 2] = topCapVertexOffset + rightIndex;
            }
        }
    }
}