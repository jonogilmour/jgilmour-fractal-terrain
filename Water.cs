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
    class Water : Landscape
    {
        private int sideLength; // Length of the sides of the landscape
        private bool debugOn;
        private float randomRange;
        private List<BoundingSphere> collisionList;
        private BoundingSphere[] collisionMatrix;
        private float scale; // Stores how large the player object is, for collision detection.

        public Water(Game game, int size, bool debug, float rng)
            : base(game)
        {
            generateIndices(size);
            this.randomRange = rng;
            this.sideLength = size;
            this.debugOn = debug;
            this.vertices = generateWaterPlane(0,0,0);         
        }

        // Return a suitable elevation for the water plane
        private float calculateHeight(float value) {
            return -value / 5.0f - 0.5f;
        }

       /* private void generateCollisions(float[,] hm, uint length)
        {
            //put a point every scale/2 points across and down
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    hm[x, y] = val;
                    if (debugOn) Debug.Print(String.Format("hm[{0},{1}] = {2}", x, y, val));
                }
            }
        }*/

        private Buffer<VertexPositionNormalColor> generateWaterPlane(float moveX, float moveY, float moveZ)
        {
            // Create a vertex array capable of storing all the triangles
            var vertexList = new List<VertexPositionNormalColor>();
            var normal = new Vector3(0, 0, 0);
            var height = calculateHeight(this.randomRange);

            // Add in the amount the plane has moved in the world since the last frame
            this.yTranslation += moveY;
            this.xTranslation += moveX;
            this.zTranslation += moveZ;

            // Fill in the points, starting at the bottom left and working right through each row of points.
            for (int z = 0; z < sideLength; z++)
            {//row z
                // Get the world Z coordinate
                var zPos = z - sideLength / 2 + zTranslation;
                for (int x = 0; x < sideLength; x++)
                {//col x
                    // Get the world X coordinate
                    var xPos = x - sideLength / 2 + xTranslation;
                    vertexList.Add(new VertexPositionNormalColor(new Vector3(xPos, height + yTranslation, zPos), normal, heightColouring(height)));
                }
            }

            // Convert the vertex list to an array and throw out the vertex list as it isn't needed anymore
            var vertexArray = vertexList.ToArray();
            vertexList = null;

            // Calculate the normals at each point
            for (int x = 0; x < indices.Length / 3; x++)
            {
                var v1 = indices[x * 3];
                var v2 = indices[x * 3 + 1];
                var v3 = indices[x * 3 + 2];

                Vector3 side1 = vertexArray[v2].Position - vertexArray[v1].Position;
                Vector3 side2 = vertexArray[v3].Position - vertexArray[v1].Position;
                normal = Vector3.Cross(side1, side2);

                vertexArray[v1].Normal += normal;
                vertexArray[v2].Normal += normal;
                vertexArray[v3].Normal += normal;
            }

            // Normalise the normals to ensure each reflects equally
            for (int x = 0; x < vertexArray.Length; x++) vertexArray[x].Normal.Normalize();

            return Buffer.Vertex.New(game.GraphicsDevice, vertexArray);
        }

        public override void Update(Matrix vMatrix, Matrix pMatrix, Matrix wMatrix, Vector3 l1_dir, Vector3 l1_colour, Vector3 l1_colour_spec, Vector3 l1_colour_amb, Vector3 movement)
        {
            basicEffect.World = wMatrix;
            basicEffect.View = vMatrix;
            basicEffect.Projection = pMatrix;
            basicEffect.DirectionalLight0.Direction = l1_dir;
            basicEffect.DirectionalLight0.DiffuseColor = l1_colour;
            basicEffect.DirectionalLight0.SpecularColor = l1_colour_spec;
            basicEffect.AmbientLightColor = l1_colour_amb;

            vertices = generateWaterPlane(movement.X, movement.Y, movement.Z);
        }

        protected override Color heightColouring(float height)
        {
            // Return transparent water colour
            return new Color(0,30,230,190);
        }

        public override void Draw(GameTime gameTime)
        {
            // Setup the vertex buffer
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Set the index buffer
            var indexBuffer = Buffer.Index.New(game.GraphicsDevice, indices);
            game.GraphicsDevice.SetIndexBuffer(indexBuffer, true);

            // Apply the basic effect technique and draw the rotating cube
            basicEffect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.SetBlendState(game.GraphicsDevice.BlendStates.AlphaBlend);
            game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, indices.Length, 0, 0);
        }
    }
}
