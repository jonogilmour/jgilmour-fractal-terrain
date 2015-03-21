using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Input;

//IMPROVEMENTS
// - Take in the default view and projection matrices instead of hardcoding them

namespace Project1
{
    using SharpDX.Toolkit.Graphics;
    abstract class Landscape : ColoredGameObject
    {
        public Landscape(Game game)
        {
            // Setup the standard basic effect
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = true,
                PreferPerPixelLighting = true,
                View = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY),
                Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f),
                World = Matrix.Identity
            };

            // Setup the basic directional light
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.01f, 0.01f, 0.01f);
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(0.01f, 0.01f, 0.01f);
            basicEffect.DirectionalLight1.Direction = -1*(new Vector3(0, -1000, 0));
            basicEffect.DirectionalLight1.Enabled = false;

            // No movement yet
            xTranslation = yTranslation = zTranslation = 0;

            // Set the input layout
            inputLayout = VertexInputLayout.FromBuffer(0, vertices);
            
            this.game = game;
        }

        protected void generateIndices(int length)
        {
            length--;
            var arrayLength = length * length * 6;
            indices = new int[arrayLength];
            int x = 0;
            for (int z = 0; z < arrayLength; z += 6)
            {
                indices[z] = x;
                indices[z + 1] = length + x + 1;
                indices[z + 2] = length + x + 2;
                indices[z + 3] = x;
                indices[z + 4] = length + x + 2;
                indices[z + 5] = x + 1;
                x++;
                var row = (z / 6) / (length + 1) + 1;
                if (x + 1 - row * (length + 1) == 0) x++;
            }
        }

        protected abstract Color heightColouring(float height);

        public override void Draw(GameTime gameTime)
        {
            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            var indexBuffer = Buffer.Index.New(game.GraphicsDevice, indices);
            game.GraphicsDevice.SetIndexBuffer(indexBuffer, true);

            // Apply the basic effect technique and draw the rotating cube
            basicEffect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, indices.Length);
        }
    }
}
