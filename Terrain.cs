using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;
using System.Diagnostics;

namespace Project1
{
    using SharpDX.Toolkit.Graphics;
    class Terrain : Landscape
    {
        public float[,] heightMap;
        private uint sideLength;
        private bool debugOn;
        private float randomRange;

        public Terrain(Game game, float size, float setHeight, bool debug, float rng)
            : base(game)
        {
            // Define the basic plane to work with

            // Create the empty heightmap
            var length = (uint)size;
            heightMap = new float[length, length];

            // Zero out the heightmap
            fillHeightmap(this.heightMap, length, 0);

            // Set the range of random numbers
            randomRange = rng;

            // Calculate the terrain heightmap
            DiamondSquare(length, randomRange, length);

            // Set the side length of the plane
            sideLength = length;

            // Set the debug mode
            debugOn = debug;

            // Fill vertex array and the index array
            generateIndices((int)length);
            vertices = generateFractalTerrain(0,0,0);
            
        }

        // Fill the heightmap with a constant value
        private void fillHeightmap(float[,] hm, uint length, float val)
        {
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    hm[x, y] = val;
                }
            }
        }

        // Fills the vertex array and calculates the normals
        private Buffer<VertexPositionNormalColor> generateFractalTerrain(float moveX, float moveY, float moveZ)
        {
            // Create a vertex array capable of storing all the vertices
            var vertexList = new List<VertexPositionNormalColor>();

            // Add in the movement of the plane in world space
            yTranslation += moveY;
            xTranslation += moveX;
            zTranslation += moveZ;

            // Initial normals are zero vectors
            var normal = new Vector3(0, 0, 0);

            // Fill in the vertices, starting at the bottom left and working right through each row of points.
            for (int z = 0; z < sideLength; z++)
            {//row z
                var zPos = z - sideLength / 2 + zTranslation;
                for (int x = 0; x < sideLength; x++)
                {//col x
                    var xPos = x - sideLength / 2 + xTranslation;
                    vertexList.Add(new VertexPositionNormalColor(new Vector3(xPos, heightMap[x, z] + yTranslation, zPos), normal, heightColouring(heightMap[x, z])));
                }
            }

            // Save the vertices into an array and throw out the list, we don't need it anymore.
            var vertexArray = vertexList.ToArray();
            vertexList = null;

            // Calculate and average the normals
            for (int x = 0; x < indices.Length / 3; x++)
            {
                var index1 = indices[x * 3];
                var index2 = indices[x * 3 + 1];
                var index3 = indices[x * 3 + 2];

                Vector3 side1 = vertexArray[index2].Position - vertexArray[index1].Position;
                Vector3 side2 = vertexArray[index3].Position - vertexArray[index1].Position;
                normal = Vector3.Cross(side1, side2);

                vertexArray[index1].Normal += normal;
                vertexArray[index2].Normal += normal;
                vertexArray[index3].Normal += normal;
            }

            // Normalise all the normal vectors
            for (int x = 0; x < vertexArray.Length; x++) vertexArray[x].Normal.Normalize();

            return Buffer.Vertex.New(game.GraphicsDevice, vertexArray);
        }

        public override void Update(Matrix vMatrix, Matrix pMatrix, Matrix wMatrix, Vector3 l1_dir, Vector3 l1_colour, Vector3 l1_colour_spec, Vector3 l1_colour_amb, Vector3 movement)
        {
            // Set all basic attributes of the basicEffect
            basicEffect.World = wMatrix;
            basicEffect.View = vMatrix;
            basicEffect.Projection = pMatrix;
            basicEffect.DirectionalLight0.Direction = l1_dir;
            basicEffect.DirectionalLight0.DiffuseColor = l1_colour;
            basicEffect.DirectionalLight0.SpecularColor = l1_colour_spec;
            basicEffect.AmbientLightColor = l1_colour_amb;

            // Generate the terrain at a new position
            vertices = generateFractalTerrain(movement.X, movement.Y, movement.Z);
        }

        // Return a random float between high and low
        private float GetRandFloat(float low, float high, Random random)
        {
            return random.NextFloat(low, high);
        }

        // Return colours based on the height of a vertex
        protected override Color heightColouring(float height)
        {
            if (height > randomRange / 9 && height < randomRange / 4) return Color.DarkGray; // Mid-mountain
            if (height > randomRange / 12 && height < randomRange / 9) return Color.Silver; // Low-mountain
            if (height > randomRange / 75 && height < randomRange / 12) return Color.DarkGreen; // Upper-Grass
            if (height > randomRange / 4) return Color.White; // Mountain caps
            if (height < -randomRange / 5) return Color.SaddleBrown; // Underwater
            else return Color.Green; // Standard grass in between
        }

        // Generate the height map at random
        void DiamondSquare(uint totalLength, float range, uint length)
        {
            if (length < 1) return;

            var random = new Random();
            var lowerBound = -range;

            // diamonds
            for (uint x = length; x < totalLength; x += length)
                for (uint z = length; z < totalLength; z += length)
                {
                    // bottom left
                    float a = this.heightMap[x - length, z - length];
                    //bottom right
                    float b = this.heightMap[x, z - length];
                    //top left
                    float c = this.heightMap[x - length, z];
                    //top right
                    float d = this.heightMap[x, z];
                    //midpoint of square
                    this.heightMap[x - length / 2, z - length / 2] = (a + b + c + d) / 4 + GetRandFloat(lowerBound, range, random);
                }

            // now have 4 diamonds, do square step
            for (uint x = length; x < totalLength; x += length)
                for (uint z = length; z < totalLength; z += length)
                {
                    //bottom left
                    float a = this.heightMap[x - length, z - length];
                    //bottom right
                    float b = this.heightMap[x, z - length];
                    //top left
                    float c = this.heightMap[x - length, z];
                    //top right
                    float d = this.heightMap[x, z];
                    //middle
                    float e = this.heightMap[x - length / 2, z - length / 2];

                    // Calculate the heights, wrap the edges by averaging the point on the opposite side
                    //left
                    this.heightMap[x - length, z - length / 2] = ((a + c + e + this.heightMap[((x - length * 1.5 < 0) ? (int)(x + length / 2) : (int)(x - length * 1.5)), z - length / 2]) / 4) + GetRandFloat(lowerBound, range, random);
                    //top
                    this.heightMap[x - length / 2, z] = ((d + c + e + this.heightMap[(x - length / 2), ((z + length / 2 >= totalLength) ? (int)(z - length / 2) : (int)(z + length / 2))]) / 4) + GetRandFloat(lowerBound, range, random);
                    //right
                    this.heightMap[x, z - length / 2] = ((d + b + e + this.heightMap[((x + length / 2 >= totalLength) ? (int)(x - length / 2) : (int)(x + length / 2)), (int)(z - length / 2)]) / 4) + GetRandFloat(lowerBound, range, random);
                    //bottom
                    this.heightMap[x - length / 2, z - length] = ((a + b + e + this.heightMap[x - length / 2, ((z - length * 1.5 < 0) ? (int)(z + length / 2) : (int)(z - length * 1.5))]) / 4) + GetRandFloat(lowerBound, range, random);
                }

            // Recur until the entire heightmap is done
            DiamondSquare(totalLength, range / 2, length / 2);
        }

    }
}
